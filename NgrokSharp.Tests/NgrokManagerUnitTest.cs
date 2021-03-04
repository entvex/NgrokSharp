using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Xunit;

namespace NgrokSharp.Tests
{
    public class NgrokManagerUnitTest : IClassFixture<NgrokManagerOneTimeSetUp>, IDisposable
    {
        string ngrokYml = "authtoken: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        
        public NgrokManagerUnitTest(NgrokManagerOneTimeSetUp ngrokManagerOneTimeSetUp)
        {
            ngrokYml = ngrokManagerOneTimeSetUp.environmentVariableNgrokYml;
        }

        public void Dispose()
        {
            foreach (var process in Process.GetProcessesByName("ngrok"))
            {
                process.Kill();
            }
        }

        [Fact]
        public void ngrokManager_ChecksDownload_True()
        {
            // ARRANGE
            var are = new AutoResetEvent(false);
            
            NgrokManager ngrokManager = new NgrokManager();
            ngrokManager.DownloadAndUnZipDone += delegate
            {
                are.Set();
            };
            
            // ACT

            ngrokManager.DownloadNgrok();
            // ASSERT
            
            
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(30));
            
            Assert.True(File.Exists("ngrok.exe"));
        }
        
        [Fact]
        public void ngrokManager_StartNgrok_True()
        {
            // ARRANGE
            WebClient webClient = new WebClient();
            webClient.DownloadFile("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip","ngrok-stable-windows-amd64.zip");

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",ngrokYml);

            NgrokManager ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok();

            // ASSERT

            var downloadedString = webClient.DownloadString("http://localhost:4040/api/");

            Assert.False(String.IsNullOrWhiteSpace(downloadedString) );
        }
        
        [Fact]
        public async void ngrokManager_StartTunnel8080_True()
        {
            // ARRANGE
            WebClient webClient = new WebClient();
            webClient.DownloadFile("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip","ngrok-stable-windows-amd64.zip");

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",ngrokYml);

            NgrokManager ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok();

            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };
            
            var startTunnel = await ngrokManager.StartTunnel(startTunnelDto);

            // ASSERT

            var downloadedString = webClient.DownloadString("http://localhost:4040/api/tunnels/foundryvtt");

            Assert.Contains("http://localhost:30000", downloadedString);
        }
    }
}