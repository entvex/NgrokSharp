﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mono.Unix;

namespace NgrokSharp.PlatformSpecific.Linux
{
    public class PlatformLinux : PlatformStrategy
    {
        public PlatformLinux()
        {
            _ngrokProcess = null;
            _downloadFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar}NgrokSharp{Path.DirectorySeparatorChar}";
        }

        public override async Task RegisterAuthTokenAsync(string authtoken)
        {
            UnixFileSystemInfo.GetFileSystemEntry($"{_downloadFolder}ngrok").FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute;
            if(_ngrokProcess == null)
            {
                using var registerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = $"{_downloadFolder}ngrok",
                        Arguments = $"authtoken {authtoken}"
                    }
                };
                registerProcess.Start();
                await registerProcess.WaitForExitAsync();
            }
            else
            {
                throw new Exception("The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.");
            }
        }

        public override void StartNgrok(string region)
        {
            UnixFileSystemInfo.GetFileSystemEntry($"{_downloadFolder}ngrok").FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute;
            if(_ngrokProcess == null)
            {
                _ngrokProcess = new Process();
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = $"{_downloadFolder}ngrok",
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
                        _ngrokProcess = new Process {StartInfo = startInfo};
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
            UnixFileSystemInfo.GetFileSystemEntry($"{_downloadFolder}ngrok").FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute;

            if(_ngrokProcess == null)
            {
                _ngrokProcess = new Process();
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = $"{_downloadFolder}ngrok",
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
                        _ngrokProcess = new Process {StartInfo = startInfo};
                    }
                }
                _ngrokProcess.Start();
            }
            else
            {
                throw new Exception("The Ngrok process is already running. Please use StopNgrok() and then StartNgrok again.");
            }
        }
    }
}