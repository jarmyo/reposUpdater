using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ReposUploader
{
    [ComImport]
    [Guid("DE9F8820-42A8-48AE-8A0D-EB49AA2010FE")]
    [TypeLibType(TypeLibTypeFlags.FLicensed)]
    public interface IDeployPack
    {
        string MainVer { get; set; }
        DateTime DateTime { get; set; }
        long Size { get; set; }
        IList<IPackFile> PackFiles { get; set; }
        IList<IPackDir> PackDirs { get; set; }
        IPublisherInfo Publisher { get; set; }
        string ProgramFullName { get; set; }
        string EntryPoint { get; set; }
    }
}