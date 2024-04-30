using System;
using System.Collections.Generic;
using ToteschaMinecraftLauncher.Scripts.Contracts;

namespace ToteschaMinecraftLauncher;

public class Modpack
{
    public string Name { get; set; }
    public string ID { get; set; }
    public string ModpackVersion { get; set; }
    public string MineceaftVersion { get; set; }
    public string ModLoader { get; set; }
    public List<ModpackFile> Files { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ConfigurationFile { get; set; }
    
}
