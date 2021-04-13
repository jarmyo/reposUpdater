using System.Runtime.InteropServices;

namespace ReposUploader
{
    [ComImport]
    [Guid("CF138F4A-5B6F-487F-920B-B8C139E5F451")]
    [TypeLibType(TypeLibTypeFlags.FLicensed)]
    public class PublisherInfo : IPublisherInfo
    {
        public extern string Name { get; set; }
        public extern string SupportLink { get; set; }
        public extern string SupportMail { get; set; }
        public extern string SupportPhone { get; set; }
        public extern string WebLink { get; set; }
    }
}