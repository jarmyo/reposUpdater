using FluentFTP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

namespace ReposUploader
{
    class Program
    {
        static bool AutoClose = true;
        static bool ForceUploadAll = false;
        static bool NoUpload = false;
        static JavaScriptSerializer JsonConvert;
        static Dictionary<string, string> GlobalParams;
        static string[] ForbbidenDirectories;
        static string[] ForbbidenExtensions;
        static void Main(string[] args)
        {
            JsonConvert = new JavaScriptSerializer();
            //the config file must be in the same location as this executable
            var configjson = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Config.json");
            GlobalParams = JsonConvert.Deserialize<Dictionary<string, string>>(configjson);

            ForbbidenDirectories = GlobalParams["ForbbidenDirectories"].Split(',');
            ForbbidenExtensions = GlobalParams["ForbbidenExtensions"].Split(',');

            //Load sensitive data from a json file

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

            CreateUpdatePAck();
        }

        private static Dictionary<string, string> newHashSet;
        private static Dictionary<string, string> prevHashSet;


        static int CounterFilesPacked = 0;
        static int CounterFilesZiped = 0;
        static int CounterFilesUploaded = 0;

        private static void PrepareDirectories()
        {
            var tempDirectory = new DirectoryInfo(Path.GetTempPath() + @"\tempOut\release");
            Debug.Write(tempDirectory.FullName);
            if (!tempDirectory.Exists)
                tempDirectory.Create();

            Console.Write("clear temp file");
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

        public static void CreateUpdatePAck()
        {

            PrepareDirectories();

            newHashSet = new Dictionary<string, string>();

            if (!ForceUploadAll)
                prevHashSet = JsonConvert.Deserialize<Dictionary<string, string>>(File.ReadAllText(GlobalParams["GlobalHashLocation"]));
            else
                prevHashSet = new Dictionary<string, string>();


            var _binaryDirectory = new DirectoryInfo(GlobalParams["binPath"]);
            
            var pack = new DeployPack();

            using (var client = new FtpClient(GlobalParams["ftpAddress"]))
            {
                if (!NoUpload)
                {
                    //change this config in order to best performance of your ftp server
                    client.Credentials = new NetworkCredential(GlobalParams["ftpUser"], GlobalParams["ftpPassword"]);
                    client.SslProtocols = SslProtocols.Tls;
                    client.ValidateAnyCertificate = true;
                    client.DataConnectionType = FtpDataConnectionType.PASV;
                    client.DownloadDataType = FtpDataType.Binary;
                    client.RetryAttempts = 5;
                    client.SocketPollInterval = 1000;
                    client.ConnectTimeout = 2000;
                    client.ReadTimeout = 2000;
                    client.DataConnectionConnectTimeout = 2000;
                    client.DataConnectionReadTimeout = 2000;
                    client.Connect();
                }

                CounterFilesPacked = 0;
                CounterFilesZiped = 0;
                CounterFilesUploaded = 0;

                var _dir = GetDir(client, _binaryDirectory, "");

                pack.PackFiles = _dir.PackFiles;
                pack.PackDirs = _dir.PackDirs;
                pack.Size = _dir.Size;

                var dt = DateTime.Now;
                pack.DateTime = dt;
                pack.MainVer = new Version(dt.Year - 2016, dt.Month, dt.Day, ((int)(dt.Hour * 60) + dt.Minute)).ToString();
                pack.EntryPoint = GlobalParams["AppEntryPoint"];
                pack.ProgramFullName = GlobalParams["AppFullName"];

                var pub = new PublisherInfo();
                pub.Name = GlobalParams["PublisherName"];
                pub.SupportLink = GlobalParams["PublisherSupport"];
                pub.SupportMail = GlobalParams["PublisherSupportMail"];
                pub.SupportPhone = GlobalParams["PublisherSupportPhone"];
                pub.WebLink = GlobalParams["PublisherWeb"];
                pack.Publisher = pub;

                var result = JsonConvert.Serialize(pack);

                File.WriteAllText(Path.GetTempPath() + @"\tempOut\local.json", result);

                if (CounterFilesUploaded > 0)
                {
                    var hashes = JsonConvert.Serialize(newHashSet);
                    File.WriteAllText(GlobalParams["GlobalHashLocation"], hashes);
                  if (!NoUpload)
                    client.UploadFile(Path.GetTempPath() + @"\tempOut\local.json", GlobalParams["ftpAppDir"] + "/local.json.deploy", FtpRemoteExists.Overwrite, false, FtpVerify.Retry);

                }
                Console.WriteLine("Ready, " + CounterFilesPacked + " checked, " + CounterFilesZiped + " zipped, " + CounterFilesUploaded + " uploaded");
                
                if (!NoUpload)
                    client.Disconnect();

                if (!AutoClose)
                    Console.ReadKey();
            }
        }
        public static PackDir GetDir(FtpClient client, DirectoryInfo _dir, string _directorio = "")
        {
            if (_directorio != "")
                _directorio += "\\";

            long _tamañoDir = 0;
            var _esteDir = new PackDir
            {
                Name = _dir.Name
            };

            var directorioAbajo = new DirectoryInfo(Path.GetTempPath() + @"\tempOut\release\" + _directorio);
            if (!directorioAbajo.Exists)
            {
                directorioAbajo.Create();
            }

            foreach (var _archivoBuild in _dir.GetFiles())
            {
                if (!ForbbidenExtensions.Contains(_archivoBuild.Extension))
                {
                    var g1 = CheckFile(client, _archivoBuild, _directorio);
                    _tamañoDir += g1.Size;
                    _esteDir.PackFiles.Add(g1);
                }
            }

            foreach (var _subdirectorioBuild in _dir.GetDirectories())
            {
                if (ForbbidenDirectories.Contains(_subdirectorioBuild.Name))
                    continue;

                var g = GetDir(client, _subdirectorioBuild, _directorio + _subdirectorioBuild.Name);
                _tamañoDir += g.Size;
                _esteDir.PackDirs.Add(g);
            }

            _esteDir.Size = _tamañoDir;

            return _esteDir;

        }

        public static PackFile CheckFile(FtpClient client, FileInfo f, string _directorio = "")
        {
            if (_directorio != "")
                _directorio += "\\";

            var _rutaTotal = Path.GetTempPath() + @"\tempOut\release\" + _directorio + f.Name;

            var p1 = new PackFile()
            {
                Name = f.Name,
                Hash = Hash(f.OpenRead()),
                Size = f.Length
            };


            newHashSet.Add(_rutaTotal, p1.Hash);
            var _esigual = prevHashSet.ContainsKey(_rutaTotal)
               && prevHashSet[_rutaTotal] == p1.Hash;

            CounterFilesPacked++;

            if (ForceUploadAll || !_esigual)
            {
                Console.WriteLine();
                Console.Write(_rutaTotal + " - " + f.Length + " . ");

                f.CopyTo(_rutaTotal);
                //////////// ZIP //////////////////
                using (ZipArchive archive = ZipFile.Open(_rutaTotal + ".zip", ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(_rutaTotal, f.Name, CompressionLevel.Optimal);
                }
                Console.Write(" zip");
                var f2 = new FileInfo(_rutaTotal);
                f2.Delete();
                CounterFilesZiped++;

                //////////// UPLOAD //////////////////

                var _rutaArriba = GlobalParams["ftpAppDir"] + "/release/" + _directorio.Replace("\\", "/") + f.Name;
                if (!NoUpload)
                {
                    client.UploadFile(_rutaTotal + ".zip", _rutaArriba + ".zip", FtpRemoteExists.Overwrite, true, FtpVerify.None);
                    CounterFilesUploaded++;
                    Console.WriteLine(" up");
                }
            }
            else
            {
                Console.Write(" . ");
            }
            //Console.WriteLine();

            return p1;
        }

        public static MD5 md5 = MD5.Create();
        public static string Hash(FileStream stream)
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

    }
}
