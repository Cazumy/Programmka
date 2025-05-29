#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        try { size += new FileInfo(file).Length; } catch { }
                    }

                    foreach (var dir in Directory.GetDirectories(current))
                    {
                        queue.Enqueue(dir);
                    }
                }
                catch { }
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
                catch (Exception) { }
            }
            foreach (string directory in Directory.GetDirectories(folderPath))
            {
                try { Directory.Delete(directory, true); }
                catch (Exception) {}
            }
        }
        public static async Task CleanAllTemp()
        {
            var tasks = AllTempPath.Select(path => Task.Run(() => CleanFolder(path)));
            await Task.WhenAll(tasks);
        }
    }
}
