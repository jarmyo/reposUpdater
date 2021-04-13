using System;
using System.Collections.Generic;

namespace ReposUploader
{
    internal class DeployPack : IDeployPack
    {
        public DeployPack()
        {
        }

        public string MainVer { get; set; }
        public DateTime DateTime { get; set; }
        public long Size { get; set; }
        public IList<IPackFile> PackFiles { get; set; }
        public IList<IPackDir> PackDirs { get; set; }
        public IPublisherInfo Publisher { get; set; }
        public string ProgramFullName { get; set; }
        public string EntryPoint { get; set; }
    }
}