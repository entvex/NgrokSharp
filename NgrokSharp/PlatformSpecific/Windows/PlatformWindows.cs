using System;
using System.Diagnostics;

namespace NgrokSharp.PlatformSpecific.Windows
{
    public class PlatformWindows : IPlatformStrategy
    {
        private Process _process;

        public PlatformWindows()
        {
            _process = new Process();
        }

        public void RegisterAuthToken(string authtoken)
        {
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "ngrok.exe",
                Arguments = $"authtoken {authtoken}"
            };
            try
            {
                _process.StartInfo = startInfo;
            }
            catch (InvalidOperationException e)
            {
                if (e.Message == "No process is associated with this object." || e.Message ==
                    "Process is already associated with a real process, so the requested operation cannot be performed.")
                    throw new Exception(
                        "The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.");
            }

            _process.Start();
        }

        public void StartNgrok(string region)
        {
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "ngrok.exe",
                Arguments = $"start --none -region {region}"
            };
            try
            {
                _process.StartInfo = startInfo;
            }
            catch (InvalidOperationException e)
            {
                if (e.Message == "No process is associated with this object.")
                    throw new Exception(
                        "The Ngrok process is already running. Please use StopNgrok() and then StartNgrok again.");

                if (e.Message ==
                    "Process is already associated with a real process, so the requested operation cannot be performed.")
                    //A process in .NET can only be created and used once, after that a new one has to be made!
                    _process = new Process();
            }

            _process.StartInfo = startInfo;
            _process.Start();
        }

        public void StopNgrok()
        {
            if (_process != null)
            {
                _process.Refresh();
                if (!_process.HasExited)
                {
                    _process.Kill();
                    _process.Close();
                }
            }
        }
    }
}