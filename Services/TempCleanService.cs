using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Programmka.Services
{
    public static class TempCleanService
    {
        public static readonly IEnumerable<string> AllTempPath = new[]
            {
            @"C:\Windows\Temp",
            @"C:\Windows\SoftwareDistribution",
            @"C:\Windows\Prefetch",
            Path.GetTempPath(),
        }.Concat(GetAllRecycleBins());

        private static IEnumerable<string> GetAllRecycleBins()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed && drive.IsReady)
                {
                    yield return Path.Combine(drive.RootDirectory.FullName, "$Recycle.Bin");
                }
            }
        }
        private static long GetFolderSize(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return 0;

            long size = 0;
            var queue = new Queue<string>();
            queue.Enqueue(folderPath);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                try
                {
                    foreach (var file in Directory.GetFiles(current))
                    {
                        try { size += new FileInfo(file).Length; }
                        catch (Exception e) { Debug.WriteLine(e.Message); }
                    }

                    foreach (var dir in Directory.GetDirectories(current))
                    {
                        queue.Enqueue(dir);
                    }
                }
                catch (Exception e) { Debug.WriteLine(e.Message); }
            }

            return size;
        }
        public static long GetFullTempSize()
        {
            long totalSize = 0;
            foreach (var path in AllTempPath)
            {
                totalSize += GetFolderSize(path);
            }
            return totalSize;
        }
        public static string NormalizeByteSize(long size)
        {
            string[] sizes = ["B", "KB", "MB", "GB", "TB"];
            float normalSize = size;
            var order = 0;
            while (normalSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                normalSize /= 1024;
            }
            return $"{normalSize:0.##} " + sizes[order];
        }
        public static void CleanFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) { return; }
            foreach (string file in Directory.GetFiles(folderPath))
            {
                try { File.Delete(file); }
                catch (Exception e) { Debug.WriteLine(e.Message); }
            }
            foreach (string directory in Directory.GetDirectories(folderPath))
            {
                try { Directory.Delete(directory, true); }
                catch (Exception e) { Debug.WriteLine(e.Message); }
            }
        }
        public static async Task CleanAllTemp()
        {
            var tasks = AllTempPath.Select(path => Task.Run(() => CleanFolder(path)));
            await Task.WhenAll(tasks);
            OpenAndCloseRecycleBin();
        }

        public static void OpenAndCloseRecycleBin() // to refresh recycle Bin icon
        {
            [DllImport("user32.dll")] static extern IntPtr FindWindow(string cls, string? title);
            [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
            [DllImport("user32.dll")] static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

            Type? shellType = Type.GetTypeFromProgID("Shell.Application");
            dynamic? shell;
            try
            {
                shell = Activator.CreateInstance(shellType!);
            } catch { return; }

            shell?.NameSpace(0x0a).Self.InvokeVerb("open"); // 0x0A = корзина

            IntPtr hwnd;
            int waited = 0;
            do
            {
                hwnd = FindWindow("CabinetWClass", null);
                if (hwnd == IntPtr.Zero) hwnd = FindWindow("ExploreWClass", null);
                Thread.Sleep(10);
                waited += 10;
            } while (hwnd == IntPtr.Zero && waited < 500);

            hwnd = FindWindow("CabinetWClass", null);
            if (hwnd == IntPtr.Zero) hwnd = FindWindow("ExploreWClass", null);
            if (hwnd == IntPtr.Zero) return;
            SetForegroundWindow(hwnd);
            Thread.Sleep(50);

            const byte ALT = 0x12, F4 = 0x73, KEYDOWN = 0, KEYUP = 2;
            keybd_event(ALT, 0, KEYDOWN, UIntPtr.Zero);
            keybd_event(F4, 0, KEYDOWN, UIntPtr.Zero);
            keybd_event(F4, 0, KEYUP, UIntPtr.Zero);
            keybd_event(ALT, 0, KEYUP, UIntPtr.Zero);
        }
    }
}
