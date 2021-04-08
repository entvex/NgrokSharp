using System;
using System.Diagnostics;

namespace NgrokSharp.PlatformSpecific.Windows
{
    public class PlatformWindows : IPlatformStrategy
    {
        private Process Process { get; set; }

        public void RegisterAuthToken(string authtoken)
        {
            if (Process != null)
            {
                Process.Refresh();
                if (!Process.HasExited)
                    throw new Exception(
                        "The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.");
            }

            Process = new Process();
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "ngrok.exe",
                Arguments = $"authtoken {authtoken}"
            };
            Process.StartInfo = startInfo;
            Process.Start();
        }

        public void StartNgrok(string region)
        {
            Process = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "ngrok.exe",
                    Arguments = $"start --none -region {region}"
                }
            };

            Process.Start();
        }

        public void StopNgrok()
        {
            if (Process != null)
            {
                Process.Refresh();
                if (!Process.HasExited) Process.Kill();
            }
        }
    }
}