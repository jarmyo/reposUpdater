using System.Runtime.InteropServices;

namespace ReposUploader
{
    [ComImport]
    [Guid("8F619BEE-824E-474F-84AD-A9A3A19A76BA")]
    [TypeLibType(TypeLibTypeFlags.FLicensed)]    
    public interface IPublisherInfo
    {
        string Name { get; set; }
        string SupportLink { get; set; }
        string SupportPhone { get; set; }
        string SupportMail { get; set; }
        string WebLink { get; set; }
    }
}