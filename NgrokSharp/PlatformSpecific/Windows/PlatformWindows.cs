#pragma warning disable CS1591
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NgrokSharp.PlatformSpecific.Windows
{
    public class PlatformWindows : PlatformStrategy
    {
        public PlatformWindows()
        {
            _ngrokProcess = null;
            _downloadFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar}NgrokSharp{Path.DirectorySeparatorChar}";
        }

        public PlatformWindows(ILogger logger)
        {
            _logger = logger;
            _ngrokProcess = null;
            _downloadFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar}NgrokSharp{Path.DirectorySeparatorChar}";
        }

        public override async Task RegisterAuthTokenAsync(string authtoken)
        {
            if (_ngrokProcess == null)
            {
                using var registerProcess = new Process();
                registerProcess.StartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = $"{_downloadFolder}ngrok.exe",
                    Arguments = $"authtoken {authtoken}"
                };
                registerProcess.Start();
                await registerProcess.WaitForExitAsync();
            }
            else
            {
                throw new Exception("The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.");
            }
        }

        /// <summary>
        /// Starts Ngrok normally 
        /// </summary>
        /// <param name="region"></param>
        public override void StartNgrok(string region)
        {
            if (_ngrokProcess == null)
            {
                _ngrokProcess = new Process();
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = $"{_downloadFolder}ngrok.exe",
                    Arguments = $"start --none -region {region}"
                };
                try
                {
                    _ngrokProcess.StartInfo = startInfo;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message == "Process is already associated with a real process, so the requested operation cannot be performed.")
                    {
                        _ngrokProcess = new Process();
                        _ngrokProcess.StartInfo = startInfo;
                    }
                }
                _ngrokProcess.Start();
            }
            else
            {
                throw new Exception("The Ngrok process is already running. Please use StopNgrok() and then StartNgrok again.");
            }
        }
        
        public override void StartNgrokWithLogging(string region)
        {
            if (_ngrokProcess == null)
            {
                _ngrokProcess = new Process();
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = $"{_downloadFolder}ngrok.exe",
                    Arguments = $"start --none -region {region} --log=stdout"
                };
                try
                {
                    _ngrokProcess.StartInfo = startInfo;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message == "Process is already associated with a real process, so the requested operation cannot be performed.")
                    {
                        _ngrokProcess = new Process();
                        _ngrokProcess.StartInfo = startInfo;
                    }
                }
                _ngrokProcess.Start();
                
                _ngrokProcess.OutputDataReceived += ProcessStandardOutput;
                _ngrokProcess.ErrorDataReceived += ProcessStandardError;
                _ngrokProcess.Start();
                _ngrokProcess.BeginOutputReadLine();
                _ngrokProcess.BeginErrorReadLine();
                
            }
            else
            {
                throw new Exception("The Ngrok process is already running. Please use StopNgrok() and then StartNgrok again.");
            }
        }
    }
}