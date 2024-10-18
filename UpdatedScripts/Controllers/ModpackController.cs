using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Controllers
{
    public class ModpackController
    {
        public OldModpack? SelectedModpack { get; private set; }
        internal async Task<Tuple<ToteschaServerDetails?, string>> GetServerDetailsAsync(string serverURL, bool forceReload)
        {
            throw new NotImplementedException();
        }

        internal void SetModpack(string modpackName)
        {
            throw new NotImplementedException();
        }
    }
}
