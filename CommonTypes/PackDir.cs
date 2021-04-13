using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ReposUploader
{
    [ComImport]
    [Guid("A7C096AB-A529-4BBD-A608-578A7FFA4F88")]
    [TypeLibType(TypeLibTypeFlags.FLicensed)]
    public class PackDir : IPackDir
    {
        public extern string Name { get; set; }
        public extern long Size { get; set; }
        public extern IList<IPackFile> PackFiles { get; set; }
        public extern IList<IPackDir> PackDirs { get; set; }
    }
}