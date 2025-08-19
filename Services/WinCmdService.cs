using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
        public static async Task RunInCMD(string command, bool visible = false)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = !visible,
                RedirectStandardError = !visible,
                UseShellExecute = visible,
                CreateNoWindow = !visible
            };

            using var process = new Process { StartInfo = startInfo };

            if (!visible)
            {
                process.Start();
                await process.WaitForExitAsync();
            }
            else
            {
                process.Start();
                process.WaitForExit();
            }
        }
        public static Task RunInCMDNoWait(string command, bool visible = false)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = true,
                CreateNoWindow = !visible
            };

            Process.Start(startInfo);
            return Task.CompletedTask;
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
                catch{}
            });
            return Task.CompletedTask;
        }

        public static void RunInPowerShell(string command, string arguments = "")
        {
            string shell = File.Exists(@"C:\Program Files\PowerShell\7\pwsh.exe") ? "pwsh.exe" : "powershell.exe";
            Process.Start(new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"{arguments} -Command \"{command}\"",
                UseShellExecute = true,
                Verb = "runas"
            });
        }
    }
}
