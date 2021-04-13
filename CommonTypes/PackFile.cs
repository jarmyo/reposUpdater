using System.Runtime.InteropServices;

namespace ReposUploader
{
    [ComImport]
    [Guid("1920190F-F98D-47CA-B548-E77B9DF343AB")]
    [TypeLibType(TypeLibTypeFlags.FLicensed)]    
    public interface IPackFile
    {
        long Size { get; set; }
        string Name { get; set; }
        string Hash { get; set; }
    }
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