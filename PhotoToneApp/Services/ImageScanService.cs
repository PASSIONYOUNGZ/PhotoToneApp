using PhotoToneApp.Models;
using System.IO;

namespace PhotoToneApp.Services
{
    public class ImageScanService
    {
        private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".bmp"
        };

        public List<ImageItem> ScanFolder(string folderPath)
        {
            List<ImageItem> images = [];

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return images;
            }

            string[] filePaths;
            try
            {
                filePaths = Directory.GetFiles(folderPath);
            }
            catch
            {
                return images;
            }

            foreach (string filePath in filePaths)
            {
                try
                {
                    string extension = Path.GetExtension(filePath);
                    if (!SupportedExtensions.Contains(extension))
                    {
                        continue;
                    }

                    FileInfo fileInfo = new(filePath);
                    if (!fileInfo.Exists)
                    {
                        continue;
                    }

                    images.Add(new ImageItem
                    {
                        FilePath = fileInfo.FullName,
                        FileName = fileInfo.Name,
                        Extension = fileInfo.Extension,
                        FileSize = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        ToneLabel = "未分析",
                        DominantColorHex = "#000000"
                    });
                }
                catch
                {
                    // Skip files that cannot be read. A single bad file should not stop the scan.
                }
            }

            return images;
        }
    }
}
