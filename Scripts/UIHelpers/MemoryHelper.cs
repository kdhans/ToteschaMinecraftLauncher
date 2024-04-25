using Hardware.Info;
using System;

namespace ToteschaMinecraftLauncher.Scripts.Helpers;

public class MemoryHelper
{
    private HardwareInfo HardwareInfo { get; set; }
    public MemoryHelper()
    {
        HardwareInfo = new HardwareInfo();
        HardwareInfo.RefreshMemoryStatus();
        HardwareInfo.RefreshOperatingSystem();
    }

    public double AvailableMemory() 
    {
        //Macs do not have a "available memory" statistic. 
        //It's generally considered to keep about 30% of RAM space if under 16GB, and 15% if over 16GB.
        if (HardwareInfo.OperatingSystem.Name.ToLower().Contains("mac"))
            return HardwareInfo.MemoryStatus.TotalPhysical - (GetMemoryReservedByPercentage());
        else
            return HardwareInfo.MemoryStatus.AvailablePhysical;
    }
    public double TotalMemory() => HardwareInfo.MemoryStatus.TotalPhysical;
    
    private double GetMemoryReservedByPercentage()
    {
        if (HardwareInfo.MemoryStatus.TotalPhysical > 16 * 1048576.0)
            return 0.15 * HardwareInfo.MemoryStatus.TotalPhysical;
        else 
            return 0.30 * HardwareInfo.MemoryStatus.TotalPhysical;
    }
}
