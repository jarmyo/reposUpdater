﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;

namespace ReposUploader
{
    internal static class Program
    {
        private static Stopwatch timer;
        private static bool AutoClose = true;
        private static bool ForceUploadAll;
        private static bool NoUpload;

        private static Dictionary<string, string> GlobalParams;
        private static Dictionary<string, string> newHashSet;
        private static Dictionary<string, string> prevHashSet;

        private static string[] ForbbidenDirectories;
        private static string[] ForbbidenExtensions;

        private static int CounterFilesChecked = 0;
        private static int CounterFilesZiped = 0;
        private static int CounterFilesUploaded = 0;


        private static void Main(string[] args)
        {
            ForceUploadAll = false;
            NoUpload = false;
            LoadConfig();
            CheckParams(args);
            CreateUpdatePack();
        }

        private static void LoadConfig()
        {

            //the config file must be in the same location as this executable
            //TODO: check if file exist
            var configjson = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Config.json");
            GlobalParams = JsonSerializer.Deserialize<Dictionary<string, string>>(configjson);
            //message if no directories or extensions are defined

            if (GlobalParams != null)
            {
                ForbbidenDirectories = GlobalParams["ForbbidenDirectories"].Split(',');
                ForbbidenExtensions = GlobalParams["ForbbidenExtensions"].Split(',');
                //TODO: if ftp password is not defined, ask for it in the console input
                //TODO: some params are required, so check.
            }
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
            string binPath = string.Empty;
            if (GlobalParams == null)
                return;

            PrepareDirectories();
            timer = Stopwatch.StartNew();
            newHashSet = new Dictionary<string, string>();

            if (!ForceUploadAll && File.Exists(GlobalParams["GlobalHashLocation"]))
            {
                prevHashSet = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(GlobalParams["GlobalHashLocation"]));
            }
            else
            {
                prevHashSet = new Dictionary<string, string>();
            }

            binPath = GlobalParams["binPath"];
            var _binaryDirectory = new DirectoryInfo(binPath);

            DeployPack pack = new();
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

            var result = JsonSerializer.Serialize(pack);

            File.WriteAllText(Path.GetTempPath() + @"\tempOut\local.json", result);

            if (CounterFilesUploaded > 0)
            {
                var hashes = JsonSerializer.Serialize(newHashSet);
                File.WriteAllText(GlobalParams["GlobalHashLocation"], hashes);
                if (!NoUpload)
                {
                    UploadToFTP(Path.GetTempPath() + @"\tempOut\local.json", "ftp://" + GlobalParams["ftpAddress"] + "/" + GlobalParams["ftpAppDir"] + "/pack.json.deploy");
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


            if (ForbbidenExtensions != null)
            {
                foreach (var _archivoBuild in _dir.GetFiles())
                {

                    if (!ForbbidenExtensions.Contains(_archivoBuild.Extension))
                    {
                        var g1 = CheckFile(_archivoBuild, _directory);
                        DirectorySize += g1.Size;
                        _thisPackDir.PackFiles.Add(g1);
                    }
                }
            }

            foreach (var _subdirectorioBuild in _dir.GetDirectories())
            {
                if (ForbbidenDirectories != null)
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

            var p1 = new PackFile()
            {
                Name = f.Name,
                Hash = Hash(f.OpenRead()),
                Size = f.Length
            };

            if (newHashSet == null)
                newHashSet = new Dictionary<string, string>();

            if (prevHashSet == null)
                prevHashSet = new Dictionary<string, string>();

            if (timer == null)
                timer = new Stopwatch();


            newHashSet.Add(completePath, p1.Hash);
            var _noFileChanges = prevHashSet.ContainsKey(completePath)
               && prevHashSet[completePath] == p1.Hash;

            CounterFilesChecked++;

            if (ForceUploadAll || !_noFileChanges)
            {
                // Console.WriteLine();
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
                    if (GlobalParams != null)
                    {
                        var _rutaArriba = "ftp://" + GlobalParams["ftpAddress"] + "/" + GlobalParams["ftpAppDir"] + "/release/" + _directory.Replace("\\", "/") + f.Name;
                        UploadToFTP(completePath + ".zip", _rutaArriba + ".zip");
                    }
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
            if (GlobalParams == null)
                return;

            ManualResetEvent waitObject;

            FtpState state = new();
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_rutaArriba);
            request.Method = WebRequestMethods.Ftp.UploadFile;
                        
            request.Credentials = new NetworkCredential(GlobalParams["ftpUser"], GlobalParams["ftpPassword"]);
            state.Request = request;
            state.FileName = completePath;

            waitObject = state.OperationComplete;

            // Asynchronously get the stream for the file contents.
            request.BeginGetRequestStream(
                new AsyncCallback(EndGetStreamCallback),
                state
            );

            // Block the current thread until all operations are complete.
            waitObject.WaitOne();

            // The operations either completed or threw an exception.
            if (state.OperationException != null)
            {
                throw state.OperationException;
            }
            else
            {
                //Console.WriteLine("The operation completed - {0}", state.StatusDescription);
            }

            CounterFilesUploaded++;
        }

        private static void EndGetStreamCallback(IAsyncResult ar)
        {
            if (ar.AsyncState == null)
                return;

            FtpState state = (FtpState)ar.AsyncState;

            Stream requestStream = null;
            // End the asynchronous call to get the request stream.
            try
            {
                requestStream = state.Request.EndGetRequestStream(ar);
                // Copy the file contents to the request stream.
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
                Console.WriteLine("-->{0}", count);
                // IMPORTANT: Close the request stream before sending the request.
                requestStream.Close();
                // Asynchronously get the response to the upload request.
                state.Request.BeginGetResponse(
                    new AsyncCallback(EndGetResponseCallback),
                    state
                );
            }

            // Return exceptions to the main application thread.
            catch (Exception e)
            {
                Console.WriteLine("Could not get the request stream.");
                state.OperationException = e;
                state.OperationComplete.Set();
                return;
            }
        }

        private static void EndGetResponseCallback(IAsyncResult ar)
        {
            if (ar.AsyncState == null)
                return;

            FtpState state = (FtpState)ar.AsyncState;
            try
            {
                var response = (FtpWebResponse)state.Request.EndGetResponse(ar);
                response.Close();

                if (response.StatusDescription == null)
                    return;

                state.StatusDescription = response.StatusDescription;
                // Signal the main application thread that
                // the operation is complete.
                state.OperationComplete.Set();
            }

            // Return exceptions to the main application thread.
            catch (Exception e)
            {
                Console.WriteLine("Error getting response.");
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