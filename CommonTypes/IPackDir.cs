using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ReposUploader
{
    [ComImport]
    [Guid("175EB158-B655-11E1-B477-02566188709B")]
    [TypeLibType(TypeLibTypeFlags.FLicensed)]    
    public interface IPackDir
    {
        string Name { get; set; }
        long Size { get; set; }
        IList<IPackFile> PackFiles { get; set; }
        IList<IPackDir> PackDirs { get; set; }
    }
}