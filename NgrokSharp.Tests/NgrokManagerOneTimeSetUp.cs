using System;
using System.Net;

namespace NgrokSharp.Tests
{
    public class NgrokManagerOneTimeSetUp : IDisposable
    {
        private readonly Uri _ngrokDownloadUrlLinux =
            new("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-linux-amd64.zip");

        private readonly Uri _ngrokDownloadUrlMac =
            new("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-darwin-amd64.zip");

        private readonly Uri _ngrokDownloadUrlWin =
            new("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip");

        public string? environmentVariableNgrokYml;

        public byte[] ngrokBytes;

        public NgrokManagerOneTimeSetUp()
        {
            var webClient = new WebClient();

            if (OperatingSystem.IsWindows()) ngrokBytes = webClient.DownloadData(_ngrokDownloadUrlWin);
            if (OperatingSystem.IsLinux()) ngrokBytes = webClient.DownloadData(_ngrokDownloadUrlLinux);
            if (OperatingSystem.IsMacOS()) ngrokBytes = webClient.DownloadData(_ngrokDownloadUrlMac);
            
            environmentVariableNgrokYml = Environment.GetEnvironmentVariable("NGROKYML",EnvironmentVariableTarget.User);
        }

        public void Dispose()
        {
        }
    }
}