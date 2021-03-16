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
        public void DownloadNgrok_CheckIfngrokEXEIsDownloaded_True()
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
            
            
            are.WaitOne(TimeSpan.FromSeconds(30));
            
            Assert.True(File.Exists("ngrok.exe"));
        }
        
        [Fact]
        public void StartNgrok_ShouldStartNgrok_True()
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
        public async void StartTunnel_StartTunnel8080_True()
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
            
            await ngrokManager.StartTunnel(startTunnelDto);

            // ASSERT

            var downloadedString = webClient.DownloadString("http://localhost:4040/api/tunnels/foundryvtt");

            Assert.Contains("http://localhost:30000", downloadedString);
        }
        
        [Fact]
        public async System.Threading.Tasks.Task StartTunnel_MissingAddrArgumentNullException_True()
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
                addr = "",
                bind_tls = "false"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => ngrokManager.StartTunnel(startTunnelDto) );

            // ASSERT

            Assert.Equal("Value cannot be null or whitespace. (Parameter 'addr')",ex.Message);
        }
        
        [Fact]
        public async System.Threading.Tasks.Task StartTunnel_MissingNameArgumentNullException_True()
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
                name = "",
                proto = "http",
                addr = "8080",
                bind_tls = "false"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => ngrokManager.StartTunnel(startTunnelDto) );

            // ASSERT

            Assert.Equal("Value cannot be null or whitespace. (Parameter 'name')",ex.Message);
        }
        
        [Fact]
        public async System.Threading.Tasks.Task StartTunnel_MissingProtoArgumentNullException_True()
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
                proto = "",
                addr = "8080",
                bind_tls = "false"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => ngrokManager.StartTunnel(startTunnelDto) );

            // ASSERT

            Assert.Equal("Value cannot be null or whitespace. (Parameter 'proto')",ex.Message);
        }
        
        [Fact]
        public async System.Threading.Tasks.Task StartTunnel_StartTunnelDTOIsNullArgumentNullException_True()
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

            StartTunnelDTO startTunnelDto = null;

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => ngrokManager.StartTunnel(startTunnelDto) );

            // ASSERT

            Assert.Equal("Value cannot be null. (Parameter 'startTunnelDto')",ex.Message);
        }
        
        [Fact]
        public async void RegisterAuthToken_ThrowsExptionUsingRegisterAuthTokenWhileAlreadyStarted_True()
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
            
            var ex = Assert.Throws<Exception>(() => ngrokManager.RegisterAuthToken("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx") );
            
            Assert.Equal("The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.",ex.Message);
        }
        
        [Fact]
        public async void RegisterAuthToken_AddNewAuthTokenAfterStop_True()
        {
            // ARRANGE
            var are = new AutoResetEvent(false);
            
            WebClient webClient = new WebClient();
            webClient.DownloadFile("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip","ngrok-stable-windows-amd64.zip");

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",ngrokYml);

            NgrokManager ngrokManager = new NgrokManager();
            ngrokManager.StartNgrok();
            
            // ACT
            
            ngrokManager.StopNgrok();
            
            ngrokManager.RegisterAuthToken("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");

            
            // ASSERT
            are.WaitOne(TimeSpan.FromSeconds(1)); // wait for the ngrok process to start and write the file
            var readAllText = File.ReadAllText($"{path.FullName}\\ngrok.yml");

            Assert.Equal("authtoken: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n",readAllText);
        }
        
    }
}