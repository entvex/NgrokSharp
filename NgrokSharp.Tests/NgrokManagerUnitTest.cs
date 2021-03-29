using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Xunit;

namespace NgrokSharp.Tests
{
    public class NgrokManagerUnitTest : IClassFixture<NgrokManagerOneTimeSetUp>, IDisposable
    {
        string _ngrokYml = "authtoken: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        private byte[] _ngrokBytes;
        public NgrokManagerUnitTest(NgrokManagerOneTimeSetUp ngrokManagerOneTimeSetUp)
        {
            _ngrokYml = ngrokManagerOneTimeSetUp.environmentVariableNgrokYml;
            _ngrokBytes = ngrokManagerOneTimeSetUp.ngrokBytes;
        }

        public void Dispose()
        {
            foreach (var process in Process.GetProcessesByName("ngrok"))
            {
                process.Kill();
            }
            //Because ngrok is only downloaded once in NgrokManagerOneTimeSetUp.
            //The File.WriteAllBytes method, can sometimes fail due killing the process and writing a new one, due to slow IO on some systems.
            //Even though I don't like it. It is a trade off between downloading ngrok every test or handling slow IO on some systems.
            //I choose to handle slow IO, and not download in every test. That is why the sleep is need here!  
            Thread.Sleep(100);
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
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

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
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

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
        
        [Theory]
        [InlineData("eu","Europe")]
        [InlineData("ap","AsiaPacific")]
        [InlineData("au","Australia")]
        [InlineData("sa","SouthAmerica")]
        [InlineData("jp","Japan")]
        [InlineData("in","India")]
        public async void StartTunnel_TestOptionalRegions_True(String regionNameShort, String regionNameFull)
        {
            // ARRANGE
            WebClient webClient = new WebClient();
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

            NgrokManager.Region parsedEnum = (NgrokManager.Region) Enum.Parse(typeof(NgrokManager.Region),regionNameFull,true);

            NgrokManager ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok(parsedEnum);

            var startTunnelDto = new StartTunnelDTO
            {
                name = "test",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };

            var httpResponseMessage = await ngrokManager.StartTunnel(startTunnelDto);

            // ASSERT
            
            var tunnelDetail =
                JsonConvert.DeserializeObject<TunnelDetail>(
                    await httpResponseMessage.Content.ReadAsStringAsync());
            
            Assert.Contains($".{regionNameShort}.", tunnelDetail.PublicUrl.ToString());
        }
        
        
        [Fact]
        public async System.Threading.Tasks.Task StartTunnel_MissingAddrArgumentNullException_True()
        {
            // ARRANGE
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

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
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

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
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

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
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

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
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

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
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

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
        
        [Fact]
        public async void StopTunnel_StopATunnelThatIsRunning_True()
        {
            // ARRANGE
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

            NgrokManager ngrokManager = new NgrokManager();
            
            ngrokManager.StartNgrok();

            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };
            
            await ngrokManager.StartTunnel(startTunnelDto);
            // ACT

            var stopTunnel = await ngrokManager.StopTunnel("foundryvtt");

            // ASSERT
            
            Assert.Equal(204, stopTunnel); // Should return 204 status code with an empty body
        }
        
        [Fact]
        public async void StopTunnel_StopTunnelNameIsNullArgumentNullException_True()
        {
            // ARRANGE
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

            NgrokManager ngrokManager = new NgrokManager();
            
            ngrokManager.StartNgrok();

            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };
            await ngrokManager.StartTunnel(startTunnelDto);
            // ACT

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => ngrokManager.StopTunnel("") );

            // ASSERT

            Assert.Equal("Value cannot be null or whitespace. (Parameter 'name')",ex.Message);
        }
        
        [Fact]
        public async void ListTunnels_StartTunnel8080AndCheckTheList_True()
        {
            // ARRANGE
            var are = new AutoResetEvent(false);
            File.WriteAllBytes("ngrok-stable-windows-amd64.zip", _ngrokBytes);

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);
            
            DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));
            
            File.WriteAllText($"{path.FullName}\\ngrok.yml",_ngrokYml);

            NgrokManager ngrokManager = new NgrokManager();
            
            ngrokManager.StartNgrok();

            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };
            
            await ngrokManager.StartTunnel(startTunnelDto);
            // ACT
            are.WaitOne(TimeSpan.FromSeconds(1)); 
            var httpResponseMessage = await ngrokManager.ListTunnels();
            
            var tunnelDetail =
                JsonConvert.DeserializeObject<TunnelsDetails>(
                    await httpResponseMessage.Content.ReadAsStringAsync());
            
            // ASSERT

            Assert.Equal("foundryvtt", tunnelDetail.Tunnels[0].Name);
        }
        
    }
}