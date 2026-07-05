using PhotoToneApp.Helpers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;

namespace PhotoToneApp.Services
{
    public class ThumbnailService
    {
        private const int MaxThumbnailSize = 220;

        public string GenerateThumbnail(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return string.Empty;
                }

                FileInfo fileInfo = new(filePath);
                string thumbnailDirectory = AppPathHelper.GetThumbnailDirectory();
                string thumbnailPath = Path.Combine(thumbnailDirectory, $"{CreateThumbnailHash(fileInfo)}.png");

                if (File.Exists(thumbnailPath))
                {
                    return thumbnailPath;
                }

                BitmapImage? thumbnailImage = LoadThumbnailImage(filePath);
                if (thumbnailImage is null)
                {
                    return string.Empty;
                }

                SaveThumbnail(thumbnailImage, thumbnailPath);

                return thumbnailPath;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static BitmapImage? LoadThumbnailImage(string filePath)
        {
            try
            {
                (int width, int height) = GetSourceImageSize(filePath);
                if (width <= 0 || height <= 0)
                {
                    return null;
                }

                using FileStream fileStream = new(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);

                BitmapImage image = new();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;

                if (width >= height)
                {
                    image.DecodePixelWidth = MaxThumbnailSize;
                }
                else
                {
                    image.DecodePixelHeight = MaxThumbnailSize;
                }

                image.StreamSource = fileStream;
                image.EndInit();
                image.Freeze();

                return image;
            }
            catch
            {
                return null;
            }
        }

        private static (int Width, int Height) GetSourceImageSize(string filePath)
        {
            try
            {
                using FileStream fileStream = new(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);

                BitmapDecoder decoder = BitmapDecoder.Create(
                    fileStream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

                BitmapFrame frame = decoder.Frames[0];

                return (frame.PixelWidth, frame.PixelHeight);
            }
            catch
            {
                return (0, 0);
            }
        }

        private static void SaveThumbnail(BitmapSource thumbnailImage, string thumbnailPath)
        {
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(thumbnailImage));

            using FileStream outputStream = new(thumbnailPath, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder.Save(outputStream);
        }

        private static string CreateThumbnailHash(FileInfo fileInfo)
        {
            string hashSource = $"{fileInfo.FullName}|{fileInfo.Length}|{fileInfo.LastWriteTimeUtc.Ticks}";
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashSource));

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
