using System;
using System.Collections.Generic;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;

namespace ToteschaMinecraftLauncher;

public class OldModpack
{
    public string Name { get; set; }
    public string ID { get; set; }
    public string ModpackVersion { get; set; }
    public string MineceaftVersion { get; set; }
    public string ModLoader { get; set; }
    public List<ModpackFile> Files { get; set; }
    public DateTime LastUpdated { get; set; }
    
}
