using ReposUploader;
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
    public partial class MainWindow
    {
        private static readonly List<string> DownloadedFiles = new List<string>();
        public MainWindow()
        {
            this.InitializeComponent();
            this.CheckComplete += this.MainWindow_CheckComplete;

            this.Loaded += (sender, e) =>
          {
              this.Start();
          };

            // Updating
            this.Title = Strings["Updating"];
        }
        public event EventHandler CheckComplete;
        public void Start()
        {
            if (local.MainVer == remote.MainVer)
            {
                // "First Install"
                this.newVersion.Text = Strings["FirstInstall"];
            }
            else
            {
                this.newVersion.Text = local.MainVer.ToString() + " → " + remote.MainVer.ToString();
            }

            Increment = 100D / remote.Size;
            BytesDownloaded = 0;

            this.CheckFiles(remote.PackFiles, local.PackFiles);

            foreach (var dirRemoto in remote.PackDirs)
            {
                var dirLocales = local.PackDirs.Where(d => d.Name == dirRemoto.Name);                
                PackDir dirLocal = dirLocales.Any() ? dirLocales.First() : null;
                this.CheckDirs(dirRemoto, dirLocal, dirRemoto.Name);
            }
        }
        private void MainWindow_CheckComplete(object sender, EventArgs e)
        {
            foreach (var newFile in DownloadedFiles)
            {
                var appfilePath = newFile.Replace(PathUpdate, PathApp);
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
        private void NotifyUpdate(long size)
        {
            BytesDownloaded += size;
            this.Progreso.Value += Increment * size;
            this._tProgreso.Text = this.Progreso.Value.ToString("#.0") + "%";
            this._tDescarga.Text = BytesDownloaded.ToString("#,#.00") + "KB /" + remote.Size.ToString("#,#.00") + "KB";

            // TODO: show the file ?
            if (this.Progreso.Value > 99)
            {
                this.CheckComplete(null, null);
            }
        }
        private async Task DownloadAndExtract(string nomDirectorio, string nomArchivo, long size)
        {
            var archivoArriba = Common.remoteStringPath + "release/" + nomDirectorio + nomArchivo + ".zip";

            var x = await Task.Run(() =>
              {
                  var esteDirectorio = PathUpdate + nomDirectorio.Replace("/", @"\");
                  var archivoAbajoExtract = esteDirectorio + @"\" + nomArchivo;
                  var archivoAbajo = archivoAbajoExtract + ".zip";

                  Utils.CreateDirectoryIfDoesntExist(esteDirectorio);

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
                          archive.ExtractToDirectory(esteDirectorio);
                          DownloadedFiles.Add(archivoAbajoExtract.Replace(@"\\", @"\"));
                      }
                  }

                  this.Dispatcher.Invoke(
                      () =>
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
    }
}