using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Contracts
{
    public class ToteschaAPIKey
    {
        public string? ID { get; set; }
        public string? Value { get; set; }
    }

    public class ToteschaAPIKeys
    {
        public List<ToteschaAPIKey>? Keys { get; set; }
    }
}
