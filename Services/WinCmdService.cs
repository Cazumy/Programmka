using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Programmka.Services
{
    public static class WinCmdService
    {
        public static async Task RunInCMD(string[] commands)
        {
            foreach (var command in commands)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();
                await process.WaitForExitAsync();
            }
        }
        public static async Task RunInCMD(string command, bool isVisible = false, bool waitForExit = true)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = isVisible,
                RedirectStandardOutput = !isVisible,
                RedirectStandardError = !isVisible,
                CreateNoWindow = !isVisible
            };

            using var process = new Process { StartInfo = startInfo };

            process.Start();

            if (waitForExit)
            {
                if (isVisible)
                    process.WaitForExit();      // синхронное ожидание
                else
                    await process.WaitForExitAsync(); // асинхронное ожидание
            }
        }

        public static Task RunBat(string script, bool visible = false)
        {
            string tempBatPath = Path.Combine(Path.GetTempPath(), "bat.bat");
            File.WriteAllText(tempBatPath, script, Encoding.Default);

            var startInfo = new ProcessStartInfo
            {
                FileName = tempBatPath,
                UseShellExecute = true,
                CreateNoWindow = !visible,
                WindowStyle = visible ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
            };
            var process = Process.Start(startInfo);

            _ = Task.Run(() =>
            {
                try
                {
                    process?.WaitForExit();
                    if (File.Exists(tempBatPath))
                    {
                        File.Delete(tempBatPath);
                    }
                }
                catch (Exception e) { MessageBox.Show(e.Message); }
            });
            return Task.CompletedTask;
        }

        public static void RunInPowerShell(string command, string arguments = "")
        {
            string shell = File.Exists(@"C:\Program Files\PowerShell\7\pwsh.exe") ? "pwsh.exe" : "powershell.exe";
            Process.Start(new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"{arguments} \"{command}\"",
                UseShellExecute = true,
                Verb = "runas"
            });
        }
    }
}
