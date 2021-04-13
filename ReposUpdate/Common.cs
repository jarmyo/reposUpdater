using ReposUploader;
using System;
using System.Collections.Generic;

namespace ReposUpdate
{
    internal static class Common
    {
        // Replace this with your info
        internal const string updateFileName = "local";
        internal const string remoteStringPath = "https://repos.mx/App/";
        internal const string InstalationPath = @"C:\Repos\";
        internal const string EntryPoint = @"repos.exe";
        internal static readonly Guid ApplicationGUID = new Guid("45A40446169340338D74048D9297426C");

        // App default directories
        internal static readonly string PathUpdate = InstalationPath + @"update\";
        internal static readonly string PathApp = InstalationPath + @"app\";
        internal static readonly string PathLogs = InstalationPath + @"logs\";

        // Deployment changes compare
        internal static DeployPack local { get; set; }
        internal static DeployPack remote { get; set; }

        // Helpers
        internal static bool DownloadAllFiles { get; set; }
        internal static double Increment { get; set; }
        internal static double BytesDownloaded { get; set; }

        // strings, change this to change to your language
        internal static readonly Dictionary<string, string> Strings = new Dictionary<string, string>
        {
            { "FirstInstall", "Primera Instalacion" },
            { "Updating", "Actualizando" },
            { "UnInstallOk", "Desinstalado Correctamente" },
        };
    }
}