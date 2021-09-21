using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using NgrokSharp.DTO;
using Xunit;

namespace NgrokSharp.Tests
{
    public class NgrokManagerUnitTest : IClassFixture<NgrokManagerOneTimeSetUp>, IDisposable
    {
        private readonly byte[] _ngrokBytes;
        private readonly string _ngrokYml = "authtoken: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        private readonly string _downloadFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar}NgrokSharp{Path.DirectorySeparatorChar}";

        public NgrokManagerUnitTest(NgrokManagerOneTimeSetUp ngrokManagerOneTimeSetUp)
        {
            _ngrokYml = ngrokManagerOneTimeSetUp.environmentVariableNgrokYml;
            _ngrokBytes = ngrokManagerOneTimeSetUp.ngrokBytes;
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
        }

        public void Dispose()
        {
            foreach (var process in Process.GetProcessesByName("ngrok")) process.Kill();
            //Because ngrok is only downloaded once in NgrokManagerOneTimeSetUp.
            //The File.WriteAllBytes method, can sometimes fail due killing the process and writing a new one, due to slow IO on some systems.
            //Even though I don't like it. It is a trade off between downloading ngrok every test or handling slow IO on some systems.
            //I choose to handle slow IO, and not download in every test. That is why the sleep is need here!  
            Thread.Sleep(100);
        }

        [Fact]
        public async Task DownloadNgrok_CheckIfNgrokIsDownloaded_True()
        {
            // ARRANGE
            var are = new AutoResetEvent(false);

            var ngrokManager = new NgrokManager();
            // ACT

            await ngrokManager.DownloadAndUnzipNgrokAsync();
            // ASSERT

            are.WaitOne(TimeSpan.FromSeconds(30));

            if (OperatingSystem.IsWindows()) Assert.True(File.Exists($"{_downloadFolder}ngrok.exe"));

            if (OperatingSystem.IsLinux()) Assert.True(File.Exists($"{_downloadFolder}ngrok"));
        }

        [Fact]
        public async Task StartNgrok_ShouldStartNgrok_True()
        {
            // ARRANGE
            var webClient = new WebClient();
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            // ASSERT
            var downloadedString = await webClient.DownloadStringTaskAsync("http://localhost:4040/api/");

            Assert.False(string.IsNullOrWhiteSpace(downloadedString));
        }

        [Fact]
        public async Task StartTunnel_StartTunnel8080_True()
        {
            // ARRANGE
            var webClient = new WebClient();
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);
            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };

            await ngrokManager.StartTunnelAsync(startTunnelDto);

            // ASSERT
            var downloadedString = await webClient.DownloadStringTaskAsync("http://localhost:4040/api/tunnels/foundryvtt");

            Assert.Contains("http://localhost:30000", downloadedString);
        }
        
        [Fact]
        public async Task StartTunnel_UseSubDomainGuid_True()
        {
            // ARRANGE
            var webClient = new WebClient();
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var newGuid = Guid.NewGuid().ToString();

            var ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);
            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "30000",
                bind_tls = "false",
                subdomain = newGuid
            };

            await ngrokManager.StartTunnelAsync(startTunnelDto);

            // ASSERT
            var downloadedString = await webClient.DownloadStringTaskAsync("http://localhost:4040/api/tunnels/foundryvtt");

            Assert.Contains(newGuid, downloadedString);
        }

        [Theory]
        [InlineData("eu", "Europe")]
        [InlineData("ap", "AsiaPacific")]
        [InlineData("au", "Australia")]
        [InlineData("sa", "SouthAmerica")]
        [InlineData("jp", "Japan")]
        [InlineData("in", "India")]
        public async Task StartTunnel_TestOptionalRegions_True(string regionNameShort, string regionNameFull)
        {
            // ARRANGE
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var parsedEnum = (NgrokManager.Region) Enum.Parse(typeof(NgrokManager.Region), regionNameFull, true);

            var ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok(parsedEnum);
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            var startTunnelDto = new StartTunnelDTO
            {
                name = "test",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };


            var httpResponseMessage = await ngrokManager.StartTunnelAsync(startTunnelDto);
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            // ASSERT
            var tunnelDetail =
                JsonSerializer.Deserialize<TunnelDetailDTO>(
                    await httpResponseMessage.Content.ReadAsStringAsync());

            Assert.Contains($".{regionNameShort}.", tunnelDetail.PublicUrl.ToString());
        }

        private DirectoryInfo SetNgrokYml()
        {
            var path = Directory.CreateDirectory(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ngrok2"));

            File.WriteAllText($"{path.FullName+Path.DirectorySeparatorChar}ngrok.yml", _ngrokYml);

            return path;
        }


        [Fact]
        public async Task StartTunnel_MissingAddrArgumentNullException_True()
        {
            // ARRANGE
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "",
                bind_tls = "false"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await ngrokManager.StartTunnelAsync(startTunnelDto));

            // ASSERT

            Assert.Equal("Value cannot be null or whitespace. (Parameter 'addr')", ex.Message);
        }

        [Fact]
        public async Task StartTunnel_MissingNameArgumentNullException_True()
        {
            // ARRANGE
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            var startTunnelDto = new StartTunnelDTO
            {
                name = "",
                proto = "http",
                addr = "8080",
                bind_tls = "false"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await ngrokManager.StartTunnelAsync(startTunnelDto));

            // ASSERT

            Assert.Equal("Value cannot be null or whitespace. (Parameter 'name')", ex.Message);
        }

        [Fact]
        public async Task StartTunnel_MissingProtoArgumentNullException_True()
        {
            // ARRANGE
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "",
                addr = "8080",
                bind_tls = "false"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await ngrokManager .StartTunnelAsync(startTunnelDto));

            // ASSERT

            Assert.Equal("Value cannot be null or whitespace. (Parameter 'proto')", ex.Message);
        }

        [Fact]
        public async Task StartTunnel_StartTunnelDTOIsNullArgumentNullException_True()
        {
            // ARRANGE
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();
            // ACT
            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ngrokManager.StartTunnelAsync(null));

            // ASSERT

            Assert.Equal("Value cannot be null. (Parameter 'startTunnelDto')", ex.Message);
        }

        [Fact]
        public async Task RegisterAuthToken_ThrowsExptionUsingRegisterAuthTokenWhileAlreadyStarted_True()
        {
            // ARRANGE
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();
            // ACT

            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            // ASSERT
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                await ngrokManager.RegisterAuthTokenAsync("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"));

            Assert.Equal(
                "The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.",
                ex.Message);
        }

        [Fact]
        public async Task RegisterAuthToken_AddNewAuthTokenAfterStop_True()
        {
            // ARRANGE
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            var are = new AutoResetEvent(false);
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            DirectoryInfo path = SetNgrokYml();
            
            var ngrokManager = new NgrokManager();
            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            // ACT
            ngrokManager.StopNgrok();
            //Wait for ngrok to stop, it can be slow on some systems.
            Thread.Sleep(1000);

            await ngrokManager.RegisterAuthTokenAsync("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");

            // ASSERT
            are.WaitOne(TimeSpan.FromSeconds(1)); // wait for the ngrok process to start and write the file

            string acualNgrokYml = null;

            if (OperatingSystem.IsWindows()) acualNgrokYml = File.ReadAllText($"{path.FullName + Path.DirectorySeparatorChar}ngrok.yml");

            if (OperatingSystem.IsLinux()) acualNgrokYml = File.ReadAllText($"{path.FullName + Path.DirectorySeparatorChar}ngrok.yml");

            Assert.Equal("authtoken: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n", acualNgrokYml);
        }

        [Fact]
        public async Task StopTunnel_StopATunnelThatIsRunning_True()
        {
            // ARRANGE
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();

            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };

            await ngrokManager.StartTunnelAsync(startTunnelDto);
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            // ACT
            var stopTunnel = await ngrokManager.StopTunnelAsync("foundryvtt");

            // ASSERT
            Assert.Equal(HttpStatusCode.NoContent,
                stopTunnel.StatusCode); // Should return 204 status code with no content
        }

        [Fact]
        public async Task StopTunnel_StopTunnelNameIsNullArgumentNullException_True()
        {
            // ARRANGE
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();

            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };
            await ngrokManager.StartTunnelAsync(startTunnelDto);
            // ACT

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await ngrokManager.StopTunnelAsync(""));
            // ASSERT

            Assert.Equal("Value cannot be null or whitespace. (Parameter 'name')", ex.Message);
        }

        [Fact]
        public async Task ListTunnels_StartTunnel8080AndCheckTheList_True()
        {
            // ARRANGE
            var are = new AutoResetEvent(false);
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
            File.WriteAllBytes($"{_downloadFolder}ngrok-stable-amd64.zip", _ngrokBytes);

            var fastZip = new FastZip();
            fastZip.ExtractZip($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, null);

            SetNgrokYml();

            var ngrokManager = new NgrokManager();

            ngrokManager.StartNgrok();
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            var startTunnelDto = new StartTunnelDTO
            {
                name = "foundryvtt",
                proto = "http",
                addr = "30000",
                bind_tls = "false"
            };

            await ngrokManager.StartTunnelAsync(startTunnelDto);
            //Wait for ngrok to start, it can be slow on some systems.
            Thread.Sleep(1000);

            // ACT
            are.WaitOne(TimeSpan.FromSeconds(1));
            var httpResponseMessage = await ngrokManager.ListTunnelsAsync();

            var tunnelDetail =
                JsonSerializer.Deserialize<TunnelsDetailsDTO>(
                    await httpResponseMessage.Content.ReadAsStringAsync());

            // ASSERT

            Assert.Equal("foundryvtt", tunnelDetail.Tunnels[0].Name);
        }
    }
}