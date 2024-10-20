﻿using System;
using System.Collections.Generic;

namespace ToteschaMinecraftLauncher;

public class OldToteschaSettings
{
    public string ServerURL { get; set; }
    public string MinecraftInstallationPath { get; set; }
    public double MemoryToAllocate { get; set; }
    public double MaxMemory { get; set; }
    public bool ForceDownload { get; set; }
    public bool DownloadOnlyServerFiles { get; set; }
    public bool CleanUpOldPacks { get; set; }
    public bool CloseLaucherAfterDownload { get; set; }
    public string LastSelectedModpack { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public List<OldModpack> InstalledModpacks { get; set; }

}
