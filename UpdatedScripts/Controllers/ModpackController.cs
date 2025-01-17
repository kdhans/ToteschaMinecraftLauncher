using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;
using ToteschaMinecraftLauncher.UpdatedScripts.Scenes;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Controllers
{
    public partial class ModpackController : Node
    {
        public Modpack? SelectedModpack { get; private set; }
        public List<Modpack>? Modpacks { get; set; }

        private LauncherWindow launcherWindow = null;
        public override void _Ready()
        {
            launcherWindow = GetNode<LauncherWindow>("/root/MainControl/LauncherWindow");
        }
        internal async Task<Tuple<ToteschaServerDetails?, string>> GetServerDetailsAsync(string serverURL, bool forceReload)
        {
            throw new NotImplementedException();
        }

        internal void SetSelectedModpack(string modpackName)
        {
            SelectedModpack = Modpacks?.SingleOrDefault(x=>x.Name == modpackName);
        }
        internal ModpackInstalledState CheckIfModpackIsUpToDate(Modpack modpack, ToteschaSettings settings)
        {
            var installedModpacks = settings.InstalledModpacks;
            if (installedModpacks == null || installedModpacks.Count() == 0)
                return ModpackInstalledState.Download;

            var installedModpackNames = installedModpacks.Select(x => x.Name).ToList();
            if (!installedModpackNames.Contains(modpack.Name))
                return ModpackInstalledState.Download;

            var selectedModpack = installedModpacks.First(x => x.Name == modpack.Name);
            if (selectedModpack.ModpackVersion != modpack.ModpackVersion)
                return ModpackInstalledState.NeedsUpdate;

            return ModpackInstalledState.UpToDate;
        }
    }
}
