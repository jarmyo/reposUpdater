using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Security.Cryptography.X509Certificates;

namespace ReposUpdate
{
    /// <summary>
    /// Actions to perform after download.
    /// </summary>
    internal static class PostInstall
    {
        public static void CreateUninstaller(string nuevaVersion, Guid AppGUID)
        {
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(
                         @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
            {
                if (parent == null)
                {
                    throw new ArgumentNullException("Uninstall registry key not found.");
                }

                try
                {
                    RegistryKey key = null;

                    try
                    {
                        string guidText = AppGUID.ToString("B");
                        key = parent.OpenSubKey(guidText, true) ??
                              parent.CreateSubKey(guidText);

                        if (key == null)
                        {
                            throw new ArgumentNullException(string.Format("Unable to create uninstaller '{0}\\{1}'", "UNINSTALL", guidText));
                        }

                        key.SetValue("DisplayName", Common.remote.ProgramFullName);
                        key.SetValue("ApplicationVersion", nuevaVersion);
                        key.SetValue("Publisher", Common.remote.Publisher.Name);
                        key.SetValue("DisplayIcon", Common.InstalationPath + AppDomain.CurrentDomain.FriendlyName);
                        key.SetValue("DisplayVersion", nuevaVersion);
                        key.SetValue("URLInfoAbout", Common.remote.Publisher.WebLink);
                        key.SetValue("HelpLink", Common.remote.Publisher.SupportLink);
                        key.SetValue("HelpTelephone", Common.remote.Publisher.SupportPhone);
                        key.SetValue("Contact", Common.remote.Publisher.SupportMail);
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", Common.InstalationPath + AppDomain.CurrentDomain.FriendlyName + @" /uninstall");
                        key.SetValue("EstimatedSize", Common.BytesDownloaded / 1024f / 1024f, RegistryValueKind.DWord);
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException(
                        "An error occurred writing uninstall information to the registry.  The service is fully installed but can only be uninstalled manually through the command line.",
                        ex);
                }
            }

            var encryptedKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Repos\Restaurante");
            if (encryptedKey != null)
            {
                encryptedKey.SetValue("version", nuevaVersion);
            }
        }

        public static void AddCertificate()
        {            
            X509Certificate2 certificate = new X509Certificate2(Common.InstalationPath + @"data\repos.cer");
            X509Store store = new X509Store(StoreName.AuthRoot, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();

            store = new X509Store(StoreName.AuthRoot, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();         
        }

        public static void CreateDesktopShorcut()
        {
            object shDesktop = "Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\" + Common.remote.ProgramFullName + ".lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = Common.remote.ProgramFullName;
            shortcut.IconLocation = Common.InstalationPath + AppDomain.CurrentDomain.FriendlyName;
            shortcut.TargetPath = Common.InstalationPath + AppDomain.CurrentDomain.FriendlyName;
            shortcut.Save();
        }
    }
}
