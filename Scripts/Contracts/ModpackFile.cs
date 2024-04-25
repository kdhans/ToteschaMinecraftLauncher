using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToteschaMinecraftLauncher.Scripts.Contracts
{
    public class ModpackFile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string FileType { get; set; }
        public string MinecraftVersion { get; set; }
        public List<string> RequiredFiles { get; set; }
        public string Filename { get; set; }
        public string InstallationLocation { get; set; }
        public string URL { get; set; }
        public string ImageURL { get; set; }
    }
}
