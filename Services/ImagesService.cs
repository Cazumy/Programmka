using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Programmka.Services
{
    public static class ImagesService
    {
        public static BitmapImage LoadImage(string pathOrPackUri)
        {
            if (string.IsNullOrWhiteSpace(pathOrPackUri))
                throw new ArgumentException("Путь не может быть пустым", nameof(pathOrPackUri));

            BitmapImage bitmap = new();

            if (pathOrPackUri.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
            {
                // WPF ресурс
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(pathOrPackUri, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }

            // Локальный файл
            if (!File.Exists(pathOrPackUri))
                throw new FileNotFoundException("Файл не найден: " + pathOrPackUri);

            using (var fs = new FileStream(pathOrPackUri, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = new MemoryStream(); // MemoryStream нужен, чтобы поток можно было закрыть
                fs.CopyTo(bitmap.StreamSource);
                bitmap.StreamSource.Position = 0;
                bitmap.EndInit();
            }

            bitmap.Freeze();
            return bitmap;
        }
    }
}
