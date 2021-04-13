using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ReposUploader
{   
    public class PackDir 
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public List<PackFile> PackFiles { get; set; }
        public List<PackDir> PackDirs { get; set; }
    }
}