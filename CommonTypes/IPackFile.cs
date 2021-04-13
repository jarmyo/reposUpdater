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
}