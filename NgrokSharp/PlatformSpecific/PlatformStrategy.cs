using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1591

namespace NgrokSharp.PlatformSpecific;

public abstract class PlatformStrategy : IDisposable
{
    protected string _downloadFolder;
    protected ILogger _logger;
    protected Process _ngrokProcess;

    public void Dispose()
    {
        _ngrokProcess.Dispose();
    }

    public abstract Task RegisterAuthTokenAsync(string authtoken);
    public abstract void StartNgrok(string region);
    public abstract void StartNgrokWithLogging(string region);

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

    protected void ProcessStandardError(object sender, DataReceivedEventArgs args)
    {
        if (!string.IsNullOrWhiteSpace(args.Data)) _logger.LogError(args.Data);
    }

    protected void ProcessStandardOutput(object sender, DataReceivedEventArgs args)
    {
        if (args == null || string.IsNullOrWhiteSpace(args.Data)) return;

        // Build structured log data
        var data = ParseLogData(args.Data);
        var logFormatData = data.Where(d => d.Key != "lvl" && d.Key != "t")
            .ToDictionary(e => e.Key, e => e.Value);
        var logFormatString = GetLogFormatString(logFormatData);
        var logLevel = ParseLogLevel(data["lvl"]);

        _logger.Log(logLevel, logFormatString, logFormatData.Values.ToArray());
    }

    private static Dictionary<string, string> ParseLogData(string input)
    {
        var result = new Dictionary<string, string>();
        var stream = new StringReader(input);
        var lastRead = 0;

        while (lastRead > -1)
        {
            // Read Key
            var keyBuilder = new StringBuilder();
            while (true)
            {
                lastRead = stream.Read();
                var c = (char)lastRead;
                if (c == '=') break;
                keyBuilder.Append(c);
            }

            // Read Value
            var valueBuilder = new StringBuilder();
            lastRead = stream.Read();
            var firstValChar = (char)lastRead;
            var quoteWrapped = false;
            if (firstValChar == '"')
            {
                quoteWrapped = true;
                lastRead = stream.Read();
                valueBuilder.Append((char)lastRead);
            }
            else
            {
                valueBuilder.Append(firstValChar);
            }

            while (true)
            {
                lastRead = stream.Read();
                if (lastRead == -1) break;

                var c = (char)lastRead;
                if (quoteWrapped && c == '"')
                {
                    lastRead = stream.Read();
                    break;
                }

                if (!quoteWrapped && c == ' ') break;
                valueBuilder.Append(c);
            }

            result.Add(keyBuilder.ToString(), valueBuilder.ToString());
        }

        return result;
    }

    private static LogLevel ParseLogLevel(string logLevelRaw)
    {
        //if (!string.IsNullOrWhiteSpace(logLevelRaw))
        //{
        //	return LogLevel.Debug;
        //}

        LogLevel logLevel;
        switch (logLevelRaw)
        {
            case "info":
                logLevel = LogLevel.Information;
                break;
            default:
                var parseResult = Enum.TryParse(logLevelRaw, out logLevel);
                if (!parseResult) logLevel = LogLevel.Debug;
                break;
        }

        return logLevel;
    }

    private string GetLogFormatString(Dictionary<string, string> logFormatData)
    {
        var logFormatSB = new StringBuilder();
        foreach (var kvp in logFormatData)
        {
            logFormatSB.Append(kvp.Key);
            logFormatSB.Append(": {");
            logFormatSB.Append(kvp.Key);
            logFormatSB.Append("} | ");
        }

        var logFormatString = logFormatSB.ToString().TrimEnd(' ').TrimEnd('|').TrimEnd(' ');
        return logFormatString;
    }
}