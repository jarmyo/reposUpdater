using System.Collections.Generic;

namespace ReposUpdate
{
    public class PackDir
    {
        public PackDir()
        {
            this.PackFiles = new List<PackFile>();
            this.PackDirs = new List<PackDir>();
        }

        public string Name { get; set; }
        public long Size { get; set; }
        public List<PackFile> PackFiles { get; set; }
        public List<PackDir> PackDirs { get; set; }
    }
}