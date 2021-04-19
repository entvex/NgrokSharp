using System;
using System.Diagnostics;
using Mono.Unix;

namespace NgrokSharp.PlatformSpecific.Linux
{
    public class PlatformLinux : IPlatformStrategy
    {
        public void RegisterAuthToken(Process process ,string authtoken)
        {
            if (process == null)
            {
                process.Refresh();
                if (!process.HasExited)
                    throw new Exception(
                        "The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.");
            }

            UnixFileSystemInfo.GetFileSystemEntry("ngrok").FileAccessPermissions =
                FileAccessPermissions.UserReadWriteExecute;

            ProcessStartInfo startInfo;
            startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "ngrok",
                Arguments = $"authtoken {authtoken}"
            };
            process.StartInfo = startInfo;
            process.Start();
        }

        public void StartNgrok(Process process ,string region)
        {
            
            if (process == null)
            {
                process.Refresh();
                if (!process.HasExited)
                    throw new Exception(
                        "The Ngrok process is already running. Please use StopNgrok() and then StartNgrok again.");
            }
            
            UnixFileSystemInfo.GetFileSystemEntry("ngrok").FileAccessPermissions =
                FileAccessPermissions.UserReadWriteExecute;

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "ngrok",
                Arguments = $"start --none -region {region}"
            };
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}