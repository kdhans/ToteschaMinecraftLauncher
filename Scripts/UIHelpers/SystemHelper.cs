using Hardware.Info;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;

namespace ToteschaMinecraftLauncher.Scripts.Helpers;

public class SystemHelper
{
    private HardwareInfo HardwareInfo { get; set; }
    public SystemHelper()
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

    public byte[] GetEnvironmentHashKey()
    {
        var putTogetherString = $"{Environment.UserName}{HardwareInfo.OperatingSystem.VersionString}{HardwareInfo.OperatingSystem.Name}";
        var builder = new StringBuilder();
        for (int i = 0; i < putTogetherString.Length / 2; i++)
        {
            builder.Append(putTogetherString[i]);
            builder.Append(putTogetherString[putTogetherString.Length - i - 1]);
        }

        List<byte> byteArray = new List<byte>();

        foreach (var character in builder.ToString())
        {
            if (byteArray.Count < 32)
                byteArray.Add(Convert.ToByte(character));
        }
        if (byteArray.Count < 32)
            for (int i = byteArray.Count; i < 32; i++)
                byteArray.Add(Convert.ToByte('$'));

        return byteArray.ToArray();
    }
    

    public byte[] GetEnvironmentIV()
    {

        var putTogetherString = $"{HardwareInfo.OperatingSystem.VersionString}{HardwareInfo.OperatingSystem.Name}";
        var builder = new StringBuilder();
        for (int i = 0; i < putTogetherString.Length / 2; i++)
        {
            builder.Append(putTogetherString[i]);
            builder.Append(putTogetherString[putTogetherString.Length - i - 1]);
        }

        List<byte> byteArray = new List<byte>();

        foreach (var character in builder.ToString())
        {
            if (byteArray.Count < 16)
                byteArray.Add(Convert.ToByte(character));
        }
        if (byteArray.Count < 16)
            for (int i = byteArray.Count; i < 16; i++)
                byteArray.Add(Convert.ToByte('#'));

        return byteArray.ToArray();
    }
}
