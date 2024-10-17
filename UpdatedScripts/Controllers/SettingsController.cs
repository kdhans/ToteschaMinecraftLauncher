using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Controllers
{
    public class SettingsController
    {
        public ToteschaSettings Settings { get; internal set; }

        internal void LoadSettings(string? settingsDirectory)
        {
            throw new NotImplementedException();
        }

        internal void SaveSettings(string? settingsDirectory)
        {
            throw new NotImplementedException();
        }

        internal void UpdateSettings(ToteschaSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
