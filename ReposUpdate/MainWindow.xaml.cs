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
    public partial class MainWindow : Window
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
                IPackDir dirLocal = dirLocales.Any() ? dirLocales.First() : null;
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

        private void CheckDefinitionFile()
        {
            if (File.Exists(PathUpdate + updateFileName + "_temp.json"))
            {
                File.Move(PathUpdate + updateFileName + "_temp.json", PathUpdate + updateFileName + ".json");
            }
            else
            {
                new WebClient().DownloadFile(remoteStringPath + updateFileName + ".json.deploy", PathUpdate + updateFileName + ".json");
            }
        }

        private void CheckDirs(IPackDir dirRemoto1, IPackDir dirlocal, string dirname = "")
        {
             if (dirname != string.Empty)
            {
                dirname += @"\";
            }

            if (dirlocal == null)
            {
                this.CheckFiles(dirRemoto1.PackFiles, null, dirname + dirRemoto1.Name);
                foreach (var dirRemoto2 in dirRemoto1.PackDirs)
                {
                    this.CheckDirs(dirRemoto2, null, dirname + dirRemoto1.Name);
                }
            }
            else
            {
                this.CheckFiles(dirRemoto1.PackFiles, dirlocal.PackFiles, dirname);

                foreach (var dirRemoto in dirRemoto1.PackDirs)
                {
                    var dirLocales = dirlocal.PackDirs.Where(d => d.Name == dirRemoto.Name);

                    if (dirLocales.Any())
                    {
                        this.CheckDirs(dirRemoto, dirLocales.First(), dirname + dirRemoto.Name);
                    }
                    else
                    {
                        this.CheckDirs(dirRemoto, null, dirname + dirRemoto.Name);
                    }
                }
            }
        }

        private void CheckFiles(IList<IPackFile> archivosRemotos, IList<IPackFile> archivosLocales, string dirname = "")
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
                foreach (var files1 in archivosRemotos)
                {
                    var localPack = archivosLocales.Where(p => p.Name == files1.Name);

                    if (DownloadAllFiles || !localPack.Any() || localPack.First().Hash != files1.Hash)
                    {
                        this.DownloadAndExtract(dirname, files1.Name, files1.Size);
                    }
                    else
                    {
                        this.NotifyUpdate(files1.Size);
                    }
                }
            }
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