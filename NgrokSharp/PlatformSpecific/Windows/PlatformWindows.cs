using System;
using System.Diagnostics;

namespace NgrokSharp.PlatformSpecific.Windows
{
    public class PlatformWindows : IPlatformStrategy
    {
        private Process _ngrokProcess;
        private readonly string _downloadFolder;

        public PlatformWindows()
        {
            _ngrokProcess = null;
            _downloadFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\NgrokSharp\\";
        }

        public void RegisterAuthToken(string authtoken)
        {
            if (_ngrokProcess == null)
            {
                Process.Start(new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = $"{_downloadFolder}ngrok.exe",
                    Arguments = $"authtoken {authtoken}"
                });
            }
            else
            {
                throw new Exception("The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.");
            }
        }

        public void StartNgrok(string region)
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

        public void StopNgrok()
        {
            if (_ngrokProcess != null)
            {
                _ngrokProcess.Refresh();
                if (!_ngrokProcess.HasExited)
                {
                    _ngrokProcess.Kill();
                    _ngrokProcess.Close();
                }
                _ngrokProcess = null;
            }
        }

        public void Dispose() => _ngrokProcess.Dispose();
    }
}