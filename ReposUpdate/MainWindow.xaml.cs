using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using static ReposUpdate.Common;

namespace ReposUpdate
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.CheckComplete += this.MainWindow_CheckComplete;

            this.Loaded += delegate
            {
                this.Start();
            };
            this.Title = Strings["Updating"]; //Updating
        }

        private void MainWindow_CheckComplete(object sender, EventArgs e)
        {
            foreach (var newFile in DownloadedFiles)
            {
                var appfilePath = newFile.Replace(Path_Update, Path_App);
                var appFile = new FileInfo(appfilePath);
                if (appFile.Exists)
                {
                    appFile.Delete();
                }
                else if (!appFile.Directory.Exists)
                {
                    appFile.Directory.Create();
                }

                File.Move(newFile, appfilePath);
            }

            this.CheckDefinitionFile();
            PostInstall.CreateUninstaller(remote.MainVer, ApplicationGUID);
            PostInstall.CreateDesktopShorcut();
            App.StartApp();
        }

        private void CheckDefinitionFile()
        {
            if (File.Exists(Path_Update + updateFileName + "_temp.json"))
            {
                File.Move(Path_Update + updateFileName + "_temp.json", Path_Update + updateFileName + ".json");
            }
            else
            {
                new WebClient().DownloadFile(remoteStringPath + updateFileName + ".json.deploy", Path_Update + updateFileName + ".json");
            }
        }

        public void Start()
        {
            if (local.MainVer == remote.MainVer)
            {
                this.newVersion.Text = Strings["FirstInstall"]; //"First Install"
            }
            else
            {
                this.newVersion.Text = local.MainVer.ToString() + " → " + remote.MainVer.ToString();
            }

            Increment = (double)100 / remote.Size;
            bytesDownloaded = 0;

            this.CheckFiles(remote.PackFiles, local.PackFiles);

            foreach (var _dirRemoto in remote.PackDirs)
            {
                var _dirLocales = local.PackDirs.Where(d => d.Name == _dirRemoto.Name);
                PackDir _dirLocal = _dirLocales.Any() ? _dirLocal = _dirLocales.First() : null;
                this.CheckDirs(_dirRemoto, _dirLocal, _dirRemoto.Name);
            }
        }

        private void CheckDirs(PackDir _dirRemoto1, PackDir _dirlocal, string dirname = "")
        {
            if (dirname != string.Empty)
            {
                dirname += @"\";
            }

            if (_dirlocal == null)
            {
                this.CheckFiles(_dirRemoto1.PackFiles, null, dirname + _dirRemoto1.Name);
                foreach (var _dirRemoto in _dirRemoto1.PackDirs)
                {
                    this.CheckDirs(_dirRemoto, null, dirname + _dirRemoto1.Name);
                }
            }
            else
            {
                this.CheckFiles(_dirRemoto1.PackFiles, _dirlocal.PackFiles, dirname);

                foreach (var _dirRemoto in _dirRemoto1.PackDirs)
                {
                    var _dirLocales = _dirlocal.PackDirs.Where(d => d.Name == _dirRemoto.Name);

                    if (_dirLocales.Any())
                    {
                        this.CheckDirs(_dirRemoto, _dirLocales.First(), dirname + _dirRemoto.Name);
                    }
                    else
                    {
                        this.CheckDirs(_dirRemoto, null, dirname + _dirRemoto.Name);
                    }
                }
            }
        }

        private void CheckFiles(List<PackFile> archivosRemotos, List<PackFile> archivosLocales, string dirname = "")
        {
            if (archivosLocales == null)
            {
                foreach (var files in archivosRemotos)
                {
                    this.DownloadAndExtract(dirname, files.Name, files.Size);
                }
            }
            else
            {
                foreach (var _files in archivosRemotos)
                {
                    var localPack = archivosLocales.Where(p => p.Name == _files.Name);

                    if (DownloadAllFiles || !localPack.Any() || localPack.First().Hash != _files.Hash)
                    {
                        this.DownloadAndExtract(dirname, _files.Name, _files.Size);
                    }
                    else
                    {
                        this.NotifyUpdate(_files.Size);
                    }
                }
            }
        }

        private void NotifyUpdate(long size)
        {
            bytesDownloaded += size;
            this.Progreso.Value += Increment * size;
            this._tProgreso.Text = this.Progreso.Value.ToString("#.0") + "%";
            this._tDescarga.Text = bytesDownloaded.ToString("#,#.00") + "KB /" + remote.Size.ToString("#,#.00") + "KB";
            //TODO: show the file ?
            if (this.Progreso.Value > 99)
            {
                this.CheckComplete(null, null);
            }
        }

        private static readonly List<string> DownloadedFiles = new List<string>();

        private async void DownloadAndExtract(string nomDirectorio, string nomArchivo, long size)
        {
            var archivoArriba = Common.remoteStringPath + "release/" + nomDirectorio + nomArchivo + ".zip";

            var x = await Task.Run(() =>
              {
                  var EsteDirectorio = Path_Update + nomDirectorio.Replace("/", @"\");
                  var archivoAbajoExtract = EsteDirectorio + @"\" + nomArchivo;
                  var archivoAbajo = archivoAbajoExtract + ".zip";

                  Utils.CreateDirectoryIfDoesntExist(EsteDirectorio);

                  if (File.Exists(archivoAbajo))
                  {
                      File.Delete(archivoAbajo);
                  }

                  if (File.Exists(archivoAbajoExtract))
                  {
                      File.Delete(archivoAbajoExtract);
                  }

                  using (var cliente = new WebClient())
                  {
                      cliente.DownloadFile(new Uri(archivoArriba), archivoAbajo);
                      using (ZipArchive archive = ZipFile.Open(archivoAbajo, ZipArchiveMode.Update))
                      {
                          archive.ExtractToDirectory(EsteDirectorio);
                          DownloadedFiles.Add(archivoAbajoExtract.Replace(@"\\", @"\"));
                      }
                  }

                  this.Dispatcher.Invoke(delegate
                  {
                      this.NotifyUpdate(size);
                  }, System.Windows.Threading.DispatcherPriority.Background);

                  new FileInfo(archivoAbajo).Delete();
                  return 0;
              });
            if (x == 0)
            {
                Logger.Write("-> " + archivoArriba);
            }
        }

        public event EventHandler CheckComplete;
    }
}