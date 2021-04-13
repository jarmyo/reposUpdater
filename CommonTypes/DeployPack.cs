using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ReposUploader
{
    [ComImport]
    [Guid("733161E9-8F92-4E58-AE5A-2BCE0DA77DB8")]
    [TypeLibType(TypeLibTypeFlags.FLicensed)]
    public class DeployPack : IDeployPack
    {
        public extern string MainVer { get; set; }
        public extern DateTime DateTime { get; set; }
        public extern long Size { get; set; }
        public extern IList<IPackFile> PackFiles { get; set; }
        public extern IList<IPackDir> PackDirs { get; set; }
        public extern IPublisherInfo Publisher { get; set; }
        public extern string ProgramFullName { get; set; }
        public extern string EntryPoint { get; set; }
    }
}