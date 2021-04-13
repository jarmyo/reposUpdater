using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows;
using static ReposUpdate.Common;

namespace ReposUpdate
{
    /// <summary>
    /// Updater.
    /// </summary>
    public class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                foreach (var arg in e.Args)
                {
                    if (arg == "/uninstall")
                    {
                        UnInstall();
                        break;
                    }
                }
            }
            else
            {
                this.InitEnvironment();

                if (this.FindNewVersion())
                {
                    Logger.Write("new version Found");
                    Current.MainWindow = new MainWindow();
                    Current.MainWindow.Show();
                }
                else
                {
                    StartApp();
                }
            }
        }

        private static void UnInstall()
        {
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(
                         @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
            {
                parent.DeleteSubKey(Common.ApplicationGUID.ToString("B"), false);

                var dclean = new System.IO.DirectoryInfo(Common.InstalationPath);
                if (!dclean.Exists)
                {
                    dclean.Create();
                }

                foreach (FileInfo file in dclean.GetFiles())
                {
                    if (file.Name == AppDomain.CurrentDomain.FriendlyName)
                    {
                        continue;
                    }

                    file.Delete();
                }

                foreach (DirectoryInfo dir in dclean.GetDirectories())
                {
                    dir.Delete(true);
                }

                MessageBox.Show(Strings["UnInstallOk"]); //Uninstalled successfully
                Process.Start(new ProcessStartInfo()
                {
                    Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Common.InstalationPath + AppDomain.CurrentDomain.FriendlyName + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                });
                Current.Shutdown();
            }
        }

        internal static void StartApp()
        {
            //the main reason fo change clickOnce its the option of run as administrator
            Process proc = new Process();
            proc.StartInfo.FileName = PathApp + EntryPoint;
            proc.StartInfo.WorkingDirectory = PathApp;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
            Current.Shutdown();
        }

        private bool FindNewVersion()
        {
            try
            {
                var JsonConvert = new JavaScriptSerializer();
                DownloadAllFiles = false;
                var remoteString = remoteStringPath + updateFileName + ".json.deploy";
                WebClient client = new WebClient();
                if (!File.Exists(PathUpdate + updateFileName + ".json"))
                {
                    Logger.Write("first instalation detected");
                    client.DownloadFile(remoteString, PathUpdate + updateFileName + "_temp.json");
                    DownloadAllFiles = true;
                    local = JsonConvert.Deserialize<DeployPack>(File.ReadAllText(PathUpdate + updateFileName + "_temp.json"));
                }
                else
                {
                    local = JsonConvert.Deserialize<DeployPack>(File.ReadAllText(PathUpdate + updateFileName + ".json"));
                }

                remote = JsonConvert.Deserialize<DeployPack>(client.DownloadString(remoteString));
                return remote.DateTime > local.DateTime || DownloadAllFiles;
            }
            catch (Exception eer)
            {
                Logger.Write("error version verify: " + eer.Message);
                File.Delete(PathUpdate + updateFileName + ".json");
                File.Delete(PathUpdate + updateFileName + "_temp.json");
            }

            return false;
        }

        private void InitEnvironment()
        {
            Utils.CreateDirectoryIfDoesntExist(InstalationPath);
            Utils.CreateDirectoryIfDoesntExist(PathLogs);
            Utils.CreateDirectoryIfDoesntExist(PathApp);
            Utils.CreateDirectoryIfDoesntExist(PathUpdate);
            Utils.CopyMeToInstallPath();
        }
    }
}