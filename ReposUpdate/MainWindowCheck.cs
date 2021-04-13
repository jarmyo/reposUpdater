using ReposUploader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using static ReposUpdate.Common;

namespace ReposUpdate
{
    public partial class MainWindow
    {
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
    }
}