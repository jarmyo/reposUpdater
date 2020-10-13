using System;
using System.Collections.Generic;

namespace ReposUpdate
{
    public class DeployPack
    {
        public DeployPack()
        {
            PackFiles = new List<PackFile>();
            PackDirs = new List<PackDir>();
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