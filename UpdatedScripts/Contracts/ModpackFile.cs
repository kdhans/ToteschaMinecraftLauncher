using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Contracts
{
    public class ModpackFile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string MinecraftVersion { get; set; }
        public string URL { get; set; }
        public string ImageURL { get; set; }
        public string DownloadURL { get; set; }
        public string Filename { get; set; }
        public string InstallationLocation { get; set; }
        public string FileStage { get; set; }
        public string ProjectID { get; set; }
        public bool ClientSide { get; set; }
        public bool ServerSide { get; set; }
        public bool RequiresZipExtraction { get; set; }
        public List<string> RequiredFiles { get; set; }
    }
}
