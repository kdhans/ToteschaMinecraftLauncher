using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using Godot;

#nullable enable
namespace ToteschaMinecraftLauncher;

public partial class WebHelper : Node
{
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
		var taskCompSource = new TaskCompletionSource<byte[]>();
		HttpRequest request = new HttpRequest();
		AddChild(request);
		request.RequestCompleted += (long result, long responseCode, string[] headers, byte[] body) => taskCompSource.TrySetResult(body);

		Error result = request.Request(url);
		var data = await taskCompSource.Task;
		if (result == Error.Ok)
		{
			try
			{
				var opt = new JsonSerializerOptions();
				opt.PropertyNameCaseInsensitive = true;
				var res = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data), opt);
				response.Data = res;
			}
			catch (Exception ex)
			{
				response.Error = $"Unable to read data at {url}. Please ensure the URL points to a JSON that can read {typeof(T)} and try again.";
			}
		}
		else
		{
			response.Error = $"Unable to connect to {url}. Please check your internet connection or try again later.";
		}
		RemoveChild(request);
		return response;
	}

	public async Task<ToteschaHttpResponse<ImageTexture>> GetImageDataAsync(string url)
	{
		if (string.IsNullOrEmpty(url))
			return new ToteschaHttpResponse<ImageTexture>() { Error = "No URL for data." };

		var response = new ToteschaHttpResponse<ImageTexture>();
		var taskCompSource = new TaskCompletionSource<byte[]>();
		HttpRequest request = new HttpRequest();
		AddChild(request);
		request.RequestCompleted += (long result, long responseCode, string[] headers, byte[] body) => taskCompSource.TrySetResult(body);

		Error result = request.Request(url);
		var data = await taskCompSource.Task;
		try
		{
			if (result == Error.Ok)
			{
				var urlString =url.ToString();
				var isPng = urlString.EndsWith(".png");
				var isJpeg = urlString.EndsWith(".jpeg") || urlString.EndsWith(".jpg");


				var image = new Image();
				Error error =  (isPng) ? image.LoadPngFromBuffer(data) : (isJpeg) ? image.LoadJpgFromBuffer(data) : Error.Unavailable;

				if (error == Error.Ok)
				{
					var texture = ImageTexture.CreateFromImage(image);
					response.Data = texture;
				}
			}
			else
			{
				response.Error = $"Unable to connect to {url}. Please check your internet connection or try again later.";
			}
		}
		catch (Exception ex)
		{ response.Error = ex.Message; }
		//request.RequestCompleted -= (long result, long responseCode, string[] headers, byte[] body) => taskCompSource.TrySetResult(body);
		RemoveChild(request);
		return response;
	}

}
