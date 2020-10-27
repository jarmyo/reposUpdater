using System;
using System.IO;
using System.Security.Cryptography;
namespace ReposUpdate
{
    internal static class Utils
    {
        internal static void CreateDirectoryIfDoesntExist(string path)
        {
            var _directorio = new DirectoryInfo(path);
            if (!_directorio.Exists)
            {
                _directorio.Create();
            }
        }

        internal static MD5 md5 = MD5.Create();

        internal static string Hash(FileStream stream)
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        internal static void CopyMeToInstallPath()
        {
            var newpath = Common.InstalationPath + AppDomain.CurrentDomain.FriendlyName;

            if (File.Exists(newpath))
            {
                var nuevo = new FileInfo(newpath);
                var or = nuevo.OpenRead();
                var _actual = Hash(or);
                or.Dispose();
                var oo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\" + AppDomain.CurrentDomain.FriendlyName);
                var or2 = oo.OpenRead();
                var nuevo1 = Hash(or2);
                or2.Dispose();
                if (_actual == nuevo1)
                {
                    return;
                }

                nuevo.Delete();
            }

            File.Copy(AppDomain.CurrentDomain.BaseDirectory + "\\" + AppDomain.CurrentDomain.FriendlyName, newpath);
        }
    }
}
