using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ReposUploader
{    
    public class DeployPack 
    {
        public DeployPack()
        {
            PackFiles = new List<PackFile>();
            PackDirs = new List<PackDir>();
            Publisher = new PublisherInfo();
        }
        public string MainVer { get; set; }
        public DateTime DateTime { get; set; }
        public long Size { get; set; }
        public List<PackFile> PackFiles { get; set; }
        public List<PackDir> PackDirs { get; set; }
        public PublisherInfo Publisher { get; set; }
        public string ProgramFullName { get; set; }
        public string EntryPoint { get; set; }
    }
}