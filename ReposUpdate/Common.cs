using System;
using System.Collections.Generic;

namespace ReposUpdate
{
    internal static partial class Common
    {
        //Replace this with your info
        internal const string updateFileName = "local";
        internal const string remoteStringPath = "https://repos.mx/App/";
        internal const string InstalationPath = @"C:\Repos\";
        internal const string EntryPoint = @"repos.exe";
        internal static Guid ApplicationGUID = new Guid("45A40446169340338D74048D9297426C");

        //App default directories
        internal static string Path_Update = InstalationPath + @"update\";
        internal static string Path_App = InstalationPath + @"app\";
        internal static string Path_Logs = InstalationPath + @"logs\";

        //Deployment changes compare
        internal static DeployPack local;
        internal static DeployPack remote;

        //Helpers
        internal static bool DownloadAllFiles = false;
        internal static double Increment = 0;
        internal static double bytesDownloaded = 0;

        //strings, change this to change to your language
        internal static Dictionary<string, string> Strings = new Dictionary<string, string>()
        {
            {"FirstInstall","Primera Instalacion" },
            {"Updating","Actualizando" },
            {"UnInstallOk","Desinstalado Correctamente" },
        };

    }
}