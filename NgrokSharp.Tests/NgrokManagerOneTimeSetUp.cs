using System;
using System.Net;

namespace NgrokSharp.Tests
{
    public class NgrokManagerOneTimeSetUp : IDisposable
    {
        public string? environmentVariableNgrokYml;

        public byte[] ngrokBytes;
        public NgrokManagerOneTimeSetUp()
        {
            WebClient webClient = new WebClient();
            ngrokBytes = webClient.DownloadData("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip");
            environmentVariableNgrokYml = Environment.GetEnvironmentVariable("ngrokYml");
        }

        public void Dispose()
        {
            
        }
    }
}