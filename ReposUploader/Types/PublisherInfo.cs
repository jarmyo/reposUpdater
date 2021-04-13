namespace ReposUploader
{
    public class PublisherInfo
    {
        private string name;
        private string webLink;
        private string supportLink;
        private string supportPhone;
        private string supportMail;
        public string Name { get => name; set => name = value; }
        public string SupportLink { get => supportLink; set => supportLink = value; }
        public string SupportPhone { get => supportPhone; set => supportPhone = value; }
        public string SupportMail { get => supportMail; set => supportMail = value; }
        public string WebLink { get => webLink; set => webLink = value; }
    }
}