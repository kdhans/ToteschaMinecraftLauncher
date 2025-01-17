using System;
using System.Collections.Generic;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;

namespace ToteschaMinecraftLauncher;

public class OldServerDetails
{
	public List<OldModpack> Modpacks { get; set; }
	public string ID { get; set; }
	public string NewsImageUrl { get; set; }
	public string News { get; set; }
}
