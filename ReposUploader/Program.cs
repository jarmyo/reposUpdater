using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Web.Script.Serialization;

namespace ReposUploader
{
    internal static class Program
    {
        private static Stopwatch timer;
        private static bool AutoClose = true;
        private static bool ForceUploadAll;
        private static bool NoUpload;
        private static readonly JavaScriptSerializer JsonConvert = new JavaScriptSerializer();

        private static Dictionary<string, string> GlobalParams;
        private static Dictionary<string, string> newHashSet;
        private static Dictionary<string, string> prevHashSet;

        private static string[] ForbbidenDirectories;
        private static string[] ForbbidenExtensions;

        private static int CounterFilesChecked;
        private static int CounterFilesZiped;
        private static int CounterFilesUploaded;

        private static void Main(string[] args)
        {
            LoadConfig();
            CheckParams(args);
            CreateUpdatePack();
        }

        private static void LoadConfig()
        {
            //the config file must be in the same location as this executable
            //TODO: check if file exist
            var configjson = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Config.json");
            GlobalParams = JsonConvert.Deserialize<Dictionary<string, string>>(configjson);
            //message if no directories or extensions are defined
            ForbbidenDirectories = GlobalParams["ForbbidenDirectories"].Split(',');
            ForbbidenExtensions = GlobalParams["ForbbidenExtensions"].Split(',');
            //TODO: if ftp password is not defined, ask for it in the console input
            //TODO: some params are required, so check.
        }

        private static void CheckParams(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg == "/force")
                {
                    ForceUploadAll = true;
                }

                if (arg == "/noupload")
                {
                    NoUpload = true;
                }

                if (arg == "/noclose")
                {
                    AutoClose = false;
                }
            }
        }

        private static void PrepareDirectories()
        {
            var tempDirectory = new DirectoryInfo(Path.GetTempPath() + @"\tempOut\release");
            Console.WriteLine("working directory: " + tempDirectory.FullName);
            if (!tempDirectory.Exists)
            {
                tempDirectory.Create();
            }
            else
            {
                Console.Write("clear temp directory");
                foreach (FileInfo file in tempDirectory.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in tempDirectory.GetDirectories())
                {
                    dir.Delete(true);
                }

                Console.WriteLine("...OK");
            }
        }

        public static void CreateUpdatePack()
        {
            PrepareDirectories();
            timer = Stopwatch.StartNew();
            newHashSet = new Dictionary<string, string>();

            if (!ForceUploadAll && File.Exists(GlobalParams["GlobalHashLocation"]))
            {
                prevHashSet = JsonConvert.Deserialize<Dictionary<string, string>>(File.ReadAllText(GlobalParams["GlobalHashLocation"]));
            }
            else
            {
                prevHashSet = new Dictionary<string, string>();
            }

            var _binaryDirectory = new DirectoryInfo(GlobalParams["binPath"]);

            DeployPack pack = new DeployPack();
            CounterFilesChecked = 0;
            CounterFilesZiped = 0;
            CounterFilesUploaded = 0;

            var _dir = GetDir(_binaryDirectory);

            pack.PackFiles = _dir.PackFiles;
            pack.PackDirs = _dir.PackDirs;
            pack.Size = _dir.Size;

            var dt = DateTime.Now;
            pack.DateTime = dt;

            //version number in repos is in base of date of compilation because we have a lot of builds
            pack.MainVer = new Version(dt.Year - 2016, dt.Month, dt.Day, (dt.Hour * 60) + dt.Minute).ToString();

            pack.EntryPoint = GlobalParams["AppEntryPoint"];
            pack.ProgramFullName = GlobalParams["AppFullName"];

            var pub = new PublisherInfo
            {
                Name = GlobalParams["PublisherName"],
                SupportLink = GlobalParams["PublisherSupport"],
                SupportMail = GlobalParams["PublisherSupportMail"],
                SupportPhone = GlobalParams["PublisherSupportPhone"],
                WebLink = GlobalParams["PublisherWeb"]
            };
            pack.Publisher = pub;

            var result = JsonConvert.Serialize(pack);

            File.WriteAllText(Path.GetTempPath() + @"\tempOut\local.json", result);

            if (CounterFilesUploaded > 0)
            {
                var hashes = JsonConvert.Serialize(newHashSet);
                File.WriteAllText(GlobalParams["GlobalHashLocation"], hashes);
                if (!NoUpload)
                {
                    UploadToFTP(Path.GetTempPath() + @"\tempOut\local.json", "ftp://" + GlobalParams["ftpAddress"] + "/" + GlobalParams["ftpAppDir"] + "/local.json.deploy");
                }
            }

            Console.WriteLine("Ready, " + CounterFilesChecked + " checked, " + CounterFilesZiped + " zipped, " + CounterFilesUploaded + " uploaded");

            if (!AutoClose)
            {
                Console.ReadKey();
            }
        }

        public static PackDir GetDir(DirectoryInfo _dir, string _directory = "")
        {
            if (_directory != string.Empty)
            {
                _directory += "\\";
            }

            long DirectorySize = 0;
            var _thisPackDir = new PackDir
            {
                Name = _dir.Name
            };

            var _localDirectory = new DirectoryInfo(Path.GetTempPath() + @"\tempOut\release\" + _directory);
            if (!_localDirectory.Exists)
            {
                _localDirectory.Create();
            }

            foreach (var _archivoBuild in _dir.GetFiles())
            {
                if (!ForbbidenExtensions.Contains(_archivoBuild.Extension))
                {
                    var g1 = CheckFile(_archivoBuild, _directory);
                    DirectorySize += g1.Size;
                    _thisPackDir.PackFiles.Add(g1);
                }
            }

            foreach (var _subdirectorioBuild in _dir.GetDirectories())
            {
                if (ForbbidenDirectories.Contains(_subdirectorioBuild.Name))
                {
                    continue;
                }

                var g = GetDir(_subdirectorioBuild, _directory + _subdirectorioBuild.Name);
                DirectorySize += g.Size;
                _thisPackDir.PackDirs.Add(g);
            }

            _thisPackDir.Size = DirectorySize;

            return _thisPackDir;
        }

        public static PackFile CheckFile(FileInfo f, string _directory = "")
        {
            if (_directory != string.Empty)
            {
                _directory += "\\";
            }

            var completePath = Path.GetTempPath() + @"\tempOut\release\" + _directory + f.Name;

            var p1 = new PackFile
            {
                Name = f.Name,
                Hash = Hash(f.OpenRead()),
                Size = f.Length
            };

            newHashSet.Add(completePath, p1.Hash);
            var _noFileChanges = prevHashSet.ContainsKey(completePath)
               && prevHashSet[completePath] == p1.Hash;

            CounterFilesChecked++;

            if (ForceUploadAll || !_noFileChanges)
            {
                Console.Write(timer.Elapsed.ToString() + " " + f.Name + " - " + f.Length + "b .");

                f.CopyTo(completePath);
                //////////// ZIP //////////////////
                using (ZipArchive archive = ZipFile.Open(completePath + ".zip", ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(completePath, f.Name, CompressionLevel.Optimal);
                }

                Console.Write(" zip");
                var f2 = new FileInfo(completePath);
                f2.Delete();
                CounterFilesZiped++;

                //////////// UPLOAD //////////////////

                if (!NoUpload)
                {
                    var _rutaArriba = "ftp://" + GlobalParams["ftpAddress"] + "/" + GlobalParams["ftpAppDir"] + "/release/" + _directory.Replace("\\", "/") + f.Name;
                    UploadToFTP(completePath + ".zip", _rutaArriba + ".zip");
                }
            }
            else
            {
                Console.Write(".");
            }

            return p1;
        }

        private static void UploadToFTP(string completePath, string _rutaArriba)
        {
            ManualResetEvent waitObject;
            FtpState state = new FtpState();
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_rutaArriba);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(GlobalParams["ftpUser"], GlobalParams["ftpPassword"]);
            state.Request = request;
            state.FileName = completePath;
            waitObject = state.OperationComplete;
            request.BeginGetRequestStream(
                new AsyncCallback(EndGetStreamCallback),
                state
            );            
            waitObject.WaitOne();            
            if (state.OperationException != null)
            {
                throw state.OperationException;
            }
            else
            {
                Console.WriteLine("OK");
            }

            CounterFilesUploaded++;
        }

        private static void EndGetStreamCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState)ar.AsyncState;
            Stream requestStream = null;
            try
            {
                requestStream = state.Request.EndGetRequestStream(ar);
                const int bufferLength = 2048;
                byte[] buffer = new byte[bufferLength];
                int count = 0;
                int readBytes = 0;
                FileStream stream = File.OpenRead(state.FileName);
                do
                {
                    readBytes = stream.Read(buffer, 0, bufferLength);
                    requestStream.Write(buffer, 0, readBytes);
                    count += readBytes;
                }
                while (readBytes != 0);
                requestStream.Close();
                state.Request.BeginGetResponse(
                    new AsyncCallback(EndGetResponseCallback),
                    state
                );
            }
            catch (Exception e)
            {
                state.OperationException = e;
                state.OperationComplete.Set();
            }
        }

        private static void EndGetResponseCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState)ar.AsyncState;
            try
            {
                var response = (FtpWebResponse)state.Request.EndGetResponse(ar);
                response.Close();
                state.StatusDescription = response.StatusDescription;
                state.OperationComplete.Set();
            }
            catch (Exception e)
            {
                state.OperationException = e;
                state.OperationComplete.Set();
            }
        }

        public static readonly SHA256 sha256 = SHA256.Create();
        public static string Hash(FileStream stream)
        {
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}