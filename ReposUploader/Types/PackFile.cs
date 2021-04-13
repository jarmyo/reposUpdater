using System.Runtime.InteropServices;

namespace ReposUploader
{    
    public class PackFile 
    {
        public long Size { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
    }
}