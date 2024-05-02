using System;
using System.Threading.Tasks;
using System.Text;
using Godot;
using Newtonsoft.Json;
using System.IO;
using System.Data.Common;
using System.Net;

#nullable enable
namespace ToteschaMinecraftLauncher;

public partial class WebHelper : Node
{
	private static System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();
	public override void _Ready()
	{
		base._Ready();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public async Task<ToteschaHttpResponse<T>> CallJsonGetRequestAsync<T>(string url)
	{
		var response = new ToteschaHttpResponse<T>();
		T data;
		try
		{
			if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && 
			   (uriResult.Scheme == Uri.UriSchemeHttp ||uriResult.Scheme == Uri.UriSchemeHttps))
			{
				_httpClient.DefaultRequestHeaders.Add("User-Agent", "ToteschaLauncher/1.0.0");
				_httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
				//_httpClient.DefaultRequestVersion = HttpVersion.Version20;
				_httpClient.Timeout = TimeSpan.FromSeconds(30);
				var httpRequest = await _httpClient.GetAsync(url);
				var stringData = await httpRequest.Content.ReadAsStringAsync();
				data = JsonConvert.DeserializeObject<T>(stringData)!;
			}
			else if (File.Exists(url))
				data = JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync(url))!;

			else
				throw new ApplicationException();
			response.Data = data;
		}
		catch
		{
			response.Error = $"Unable to read data at {url}. Please ensure the URL points to a JSON that can read {typeof(T)} and try again.";
		}
		return response;
	}

	public async Task<ToteschaHttpResponse<ImageTexture>> GetImageDataAsync(string url)
	{
		if (string.IsNullOrEmpty(url))
			return new ToteschaHttpResponse<ImageTexture>() { Error = "No URL for data." };

		var response = new ToteschaHttpResponse<ImageTexture>();
		try
		{
			var httpRequest = await _httpClient.GetAsync(url);
			var byteResult = await httpRequest.Content.ReadAsByteArrayAsync();			

			var isPng = url.ToLower().EndsWith("png");
			var isJpeg = url.ToLower().EndsWith("jpeg");
			var isWebp = url.ToLower().EndsWith("webp"); 
			
			var image = new Image();
			
			Error error;

			error = (isPng) ? image.LoadPngFromBuffer(byteResult) : 
					(isJpeg) ? image.LoadJpgFromBuffer(byteResult) : 
					(isWebp) ? image.LoadWebpFromBuffer(byteResult) :
					Error.Unavailable;

			if (image == null || error != Error.Ok)
				throw new NullReferenceException();
			
			
			response.Data = ImageTexture.CreateFromImage(image);
		}
		catch
		{
			response.Error = $"Unable to connect to {url}. Please check your internet connection or try again later.";
		}

		return response;
	}
}
