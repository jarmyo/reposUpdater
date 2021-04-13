using System.Runtime.InteropServices;

namespace ReposUploader
{
    [ComImport]
    [Guid("F5C7CB80-2ACA-45F8-9626-79C79667CAAC")]
    [TypeLibType(TypeLibTypeFlags.FLicensed)]
    public class PackFile : IPackFile
    {
        public extern long Size { get; set; }
        public extern string Name { get; set; }
        public extern string Hash { get; set; }
    }
}