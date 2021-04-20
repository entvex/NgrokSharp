using System;
using System.Diagnostics;
using Mono.Unix;

namespace NgrokSharp.PlatformSpecific.Linux
{
    public class PlatformLinux : IPlatformStrategy
    {
        public void RegisterAuthToken(Process process ,string authtoken)
        {
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
            try
            {
                process.StartInfo = startInfo;
            }
            catch (InvalidOperationException e)
            {
                if (e.Message == "No process is associated with this object." || e.Message == "Process is already associated with a real process, so the requested operation cannot be performed.")
                {
                    throw new Exception(
                        "The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.");
                }
            }
            process.Start();
        }

        public void StartNgrok(Process process ,string region)
        {
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
            try
            {
                process.StartInfo = startInfo;
            }
            catch (InvalidOperationException e)
            {
                if (e.Message == "No process is associated with this object." || e.Message == "Process is already associated with a real process, so the requested operation cannot be performed.")
                {
                    throw new Exception(
                        "The Ngrok process is already running. Please use StopNgrok() and then StartNgrok again.");
                }
            }
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}