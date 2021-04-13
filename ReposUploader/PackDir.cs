using System.Collections.Generic;

namespace ReposUploader
{
    internal class PackDir : IPackDir
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public IList<IPackFile> PackFiles { get; set; }
        public IList<IPackDir> PackDirs { get; set; }
    }
}