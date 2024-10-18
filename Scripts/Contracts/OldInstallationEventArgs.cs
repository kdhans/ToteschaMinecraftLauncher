using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToteschaMinecraftLauncher.Scripts.Contracts
{
    public class OldInstallationEventArgs : EventArgs
    {
        public float InstallationPercentage { get; set; }
        public string InstallationStatus { get; set; }
        public OldInstallationEventArgs(float InstallationPercentage, string InstallationStatus)
        { 
            this.InstallationPercentage = InstallationPercentage;
            this.InstallationStatus = InstallationStatus;
        }
    }
}
