# NgrokSharp

NgrokSharp is a .NET library to manage the official [Ngrok client](https://ngrok.com/).

# Usage

## Reverse http(s) proxy on port 8080 
```csharp
static async Task Main(string[] args)
{
    INgrokManager _ngrokManager;
    _ngrokManager = new NgrokManager();

    await _ngrokManager.DownloadAndUnzipNgrokAsync();

    // Insert your token, if you have one.
    //await _ngrokManager.RegisterAuthTokenAsync("Your token");

    _ngrokManager.StartNgrok();

    var tunnel = new StartTunnelDTO
    {
        name = "reverse proxy",
        proto = "http",
        addr = "8080"
    };

    var httpResponseMessage = await _ngrokManager.StartTunnelAsync(tunnel);

    if ((int)httpResponseMessage.StatusCode == 201)
    {
        var tunnelDetail =
            JsonSerializer.Deserialize<TunnelDetailDTO>(
                await httpResponseMessage.Content.ReadAsStringAsync());

        Console.WriteLine(tunnelDetail.PublicUrl);
    }
}
```