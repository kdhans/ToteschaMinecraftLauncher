using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

namespace ToteschaMinecraftLauncher.Scripts.UIHelpers
{
	public class LoginHelper
	{
		private record PPFTLoginRecord(string PPFTValue, string LoginURL);
		private record LiveOAuthToken(string accessToken, string refreshToken);
		private record XboxLiveToken(string token, string userHash);
		private record XtsToken(string token, string userHash);
		private record MinecraftProfile(string uuid, string name);
		public record MinecraftSession(string username, string uuid, string accessToken, string userType, string xboxUserId);
		private class XboxTokenRequest
		{
			public XboxTokenProperties Properties { get; set; }
			public string RelyingParty { get; set; }
			public string TokenType { get; set; }
		}
		private class XboxTokenProperties
		{
			public string AuthMethod { get; set; }
			public string SiteName { get; set; }
			public string RpsTicket { get; set; }
		}
		private class XtsTokenRequest
		{
			public XtsProperties Properties { get; set; }
			public string RelyingParty { get; set; }
			public string TokenType { get; set; }
		}
		private class XtsProperties
		{
			public string SandboxId { get; set; }
			public string[] UserTokens { get; set; }
		}
		private class MinecraftBearerRequest
		{
			public string identityToken { get; set; }
			public bool ensureLegacyEnabled { get; set; }
		}

		private static async Task<PPFTLoginRecord> GetPPFTLoginRecordAsync(HttpClient client)
		{
			string ppftValue = string.Empty, loginUrl = string.Empty;
			try
			{
				using (var result = await client.GetAsync("https://login.live.com/oauth20_authorize.srf?client_id=000000004C12AE6F&redirect_uri=https://login.live.com/oauth20_desktop.srf&scope=service::user.auth.xboxlive.com::MBI_SSL&display=touch&response_type=token&locale=en"))
				{
					var rawData = await result.Content.ReadAsStringAsync();
					ppftValue = Regex.Match(rawData, "value=\"(.+?)\"").Value;
					ppftValue = ppftValue.Replace("value=\"", string.Empty);
					ppftValue = ppftValue.TrimEnd('"');

					loginUrl = Regex.Match(rawData, "urlPost:'(.+?)'").Value;
					loginUrl = loginUrl.Replace("urlPost:'", string.Empty);
					loginUrl = loginUrl.TrimEnd('\'');
				}
			}
			catch
			{
				throw new ApplicationException("Unable to get oauth login code. Microsoft services may be currently down.");
			}

			return new PPFTLoginRecord(ppftValue, loginUrl);
		}
		private static async Task<LiveOAuthToken> GetLiveOAuthTokenAsync(HttpClient client, PPFTLoginRecord loginRecord, string username, string password)
		{
			string resultString = string.Empty;
			var resultDictionary = new Dictionary<string, string>();
			var requestDict = new Dictionary<string, string>
			{
				{ "login", username },
				{ "loginmft", username },
				{ "passwd", password },
				{ "PPFT", loginRecord.PPFTValue }
			};

			try
			{
				using (var request = new HttpRequestMessage(HttpMethod.Post, loginRecord.LoginURL) { Content = new FormUrlEncodedContent(requestDict), })
				using (var result = await client.SendAsync(request))
				{
					var redirect = result.RequestMessage?.RequestUri?.ToString();
					if (redirect != null && redirect != loginRecord.LoginURL)
						resultString = redirect;
				}
				var queryString = resultString.Split('#')[1];
				var tokens = queryString.Split("&");
				foreach (var token in tokens)
					resultDictionary.Add(token.Split("=")[0], token.Split("=")[1]);
			}
			catch
			{
				throw new AuthenticationException("Unable to login to Microsoft Live. Please check your username and password and try again.");
			}

			return new LiveOAuthToken(resultDictionary["access_token"], resultDictionary["refresh_token"]);
		}
		private static async Task<XboxLiveToken> GetXboxLiveTokenAsync(HttpClient client, LiveOAuthToken liveOAuthToken)
		{
			string token, userId;
			var tokenRequest = new XboxTokenRequest()
			{
				Properties = new XboxTokenProperties()
				{
					AuthMethod = "RPS",
					SiteName = "user.auth.xboxlive.com",
					RpsTicket = liveOAuthToken.accessToken
				},
				RelyingParty = "http://auth.xboxlive.com",
				TokenType = "JWT"
			};

			var content = new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json");
			try
			{
				var result = await client.PostAsync("https://user.auth.xboxlive.com/user/authenticate", content);
				var resultData = await result.Content.ReadAsStringAsync();
				dynamic resultAsJson = JsonConvert.DeserializeObject<dynamic>(resultData!)!;
				token = resultAsJson.Token;
				userId = resultAsJson.DisplayClaims.xui[0].uhs;
			}
			catch
			{
				throw new ApplicationException("Unable to authenticate to Xbox Live. Xbox services may be down.");
			}

			return new XboxLiveToken(token, userId);
		}
		private static async Task<XtsToken> GetXboxXtsTokenAsync(HttpClient client, XboxLiveToken xboxLiveToken)
		{
			string token, userId;
			var tokenRequest = new XtsTokenRequest()
			{
				Properties = new XtsProperties()
				{
					SandboxId = "RETAIL",
					UserTokens = new string[] { xboxLiveToken.token }
				},
				RelyingParty = "rp://api.minecraftservices.com/",
				TokenType = "JWT"
			};

			var content = new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json");
			try
			{
				var result = await client.PostAsync("https://xsts.auth.xboxlive.com/xsts/authorize", content);
				var resultData = await result.Content.ReadAsStringAsync();
				dynamic resultAsJson = JsonConvert.DeserializeObject<dynamic>(resultData!)!;
				token = resultAsJson.Token;
				userId = resultAsJson.DisplayClaims.xui[0].uhs;
			}
			catch
			{
				throw new ApplicationException("Unable to authenticate to Xbox XSTS services. Xbox services may be down.");
			}
			return new XtsToken(token, userId);
		}
		private static async Task<string> GetMinecraftAccessToken(HttpClient client, XtsToken xtsToken)
		{
			string accessToken;
			try
			{
				var tokenRequest = new MinecraftBearerRequest()
				{
					ensureLegacyEnabled = true,
					identityToken = $"XBL3.0 x={xtsToken.userHash};{xtsToken.token}"
				};
				var content = new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json");
				var result = await client.PostAsync("https://api.minecraftservices.com/authentication/login_with_xbox", content);
				var resultData = await result.Content.ReadAsStringAsync();
				dynamic resultAsJson = JsonConvert.DeserializeObject<dynamic>(resultData!)!;
				accessToken = resultAsJson.access_token;
			}
			catch
			{
				throw new ApplicationException("Unable to authenticate to Minecraft services. Mojang services may be down, or the user may not have purchased Minecraft JE.");
			}
			return accessToken;
		}
		private static async Task<MinecraftProfile> GetMinecraftProfileAsync(HttpClient client, string minecraftBearerToken)
		{
			string userID = string.Empty, name = string.Empty;
			try
			{
				using (var request = new HttpRequestMessage(HttpMethod.Get, "https://api.minecraftservices.com/minecraft/profile"))
				{
					request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", minecraftBearerToken);
					using (var result = await client.SendAsync(request))
					{
						var resultData = await result.Content.ReadAsStringAsync();
						dynamic resultAsJson = JsonConvert.DeserializeObject<dynamic>(resultData!)!;
						userID = resultAsJson.id;
						name = resultAsJson.name;
					}
				}
			}
			catch
			{
				throw new ApplicationException("Unable to authenticate to Minecraft services. Mojang services may be down, or the user may not have a Minecraft JE profile.");
			}
			return new MinecraftProfile(userID, name);
		}
		private static string GetXuidFromToken(string token)
		{
			string xuid;
			try
			{
				var handler = new JwtSecurityTokenHandler();
				var decodedValue = handler.ReadJwtToken(token);
				xuid = decodedValue!.Payload["xuid"]!.ToString()!;
			}
			catch
			{
				throw new ApplicationException("Invalid Xuid token recieved from Mojang. Mojang/Minecraft services may not be online right now - please try again later.");
			}
			return xuid;
		}

		public static async Task<MinecraftSession> GetMinecraftSession(string username, string password)
		{
			MinecraftProfile profile;
			string minecraftToken;
			XtsToken xtsToken;
			XboxLiveToken xboxLive;
			LiveOAuthToken liveOauth;
			PPFTLoginRecord ppft;
			using (var client = new HttpClient())
			{
				ppft = await GetPPFTLoginRecordAsync(client);
				liveOauth = await GetLiveOAuthTokenAsync(client, ppft, username, password);
				xboxLive = await GetXboxLiveTokenAsync(client, liveOauth);
				xtsToken = await GetXboxXtsTokenAsync(client, xboxLive);
				minecraftToken = await GetMinecraftAccessToken(client, xtsToken);
				profile = await GetMinecraftProfileAsync(client, minecraftToken);
			}
			return new MinecraftSession(profile.name, profile.uuid, minecraftToken, "msa", GetXuidFromToken(minecraftToken));
		}

	}
}
