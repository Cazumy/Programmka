#pragma warning disable CS8618

using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace Programmka.Services;

public static class AppUpdaterService
{
    private const string VersionCheckUrl = "https://raw.githubusercontent.com/Cazumy/Programmka/refs/heads/main/version.json";

    public static async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        using var client = new HttpClient();
        var json = await client.GetStringAsync(VersionCheckUrl);
        var info = JsonSerializer.Deserialize<UpdateInfo>(json);

        var current = typeof(AppUpdaterService).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        return Version.Parse(info!.Version) > Version.Parse(current) ? info : null;
    }

    public static async Task ApplyUpdateAsync(UpdateInfo info)
    {
        string tempNewExe = Path.Combine(Path.GetTempPath(), "app_new.exe");
        string? currentExe = Environment.ProcessPath;
        string batchFile = Path.Combine(Path.GetTempPath(), "update.bat");

        using var client = new HttpClient();
        await using (var remote = await client.GetStreamAsync(info.DownloadUrl))
        await using (var local = File.Create(tempNewExe))
            await remote.CopyToAsync(local);

        string batContent = $"""
@echo off
:loop
tasklist | find /i "{Path.GetFileName(currentExe)}" >nul
if not errorlevel 1 (
    timeout /t 1 >nul
    goto loop
)
copy /y "{tempNewExe}" "{currentExe}"
start "" "{currentExe}"
del "%~f0"
""";

        await File.WriteAllTextAsync(batchFile, batContent);

        await WinCmdService.RunInCMDNoWait(batchFile);

        Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
    }

    public class UpdateInfo
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }
    }
}
