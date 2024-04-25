using CmlLib.Core.Auth.Microsoft;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XboxAuthNet.Game.Msal;
using Godot;

namespace ToteschaMinecraftLauncher.Scripts.Logic
{
	public partial class LauncherLogic : Node
	{
		private static string _clientID = "";
		public LauncherLogic() { }
		//1) Login the user
		public static async Task LoginUser()
		{
			var app = await MsalClientHelper.BuildApplicationWithCache(_clientID);
			var loginHandler = JELoginHandlerBuilder.BuildDefault();

			var authenticator = loginHandler.CreateAuthenticatorWithNewAccount(default);
			authenticator.AddMsalOAuth(app, msal => msal.Interactive());
			authenticator.AddXboxAuthForJE(xbox => xbox.Basic());
			authenticator.AddJEAuthenticator();
			var session = await authenticator.ExecuteForLauncherAsync();
		}

		//2) 
	}
}
