using PhotoToneApp.Helpers;
using PhotoToneApp.Models;
using System.IO;
using System.Text.Json;

namespace PhotoToneApp.Services
{
    public class JsonCacheService
    {
        private const string CacheFileName = "cache.json";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        private CacheData _cacheData = new();

        public CacheData LoadCache()
        {
            try
            {
                string cacheFilePath = GetCacheFilePath();
                if (!File.Exists(cacheFilePath))
                {
                    _cacheData = new CacheData();
                    return _cacheData;
                }

                string json = File.ReadAllText(cacheFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _cacheData = new CacheData();
                    return _cacheData;
                }

                _cacheData = JsonSerializer.Deserialize<CacheData>(json, _jsonOptions) ?? new CacheData();
                _cacheData.Images ??= [];

                return _cacheData;
            }
            catch
            {
                _cacheData = new CacheData();
                return _cacheData;
            }
        }

        public void SaveCache(IEnumerable<ImageItem> images)
        {
            try
            {
                CacheData cacheData = new()
                {
                    Images = images
                        .Where(image => File.Exists(image.FilePath))
                        .Select(CloneForCache)
                        .ToList()
                };

                string cacheFilePath = GetCacheFilePath();
                string json = JsonSerializer.Serialize(cacheData, _jsonOptions);
                File.WriteAllText(cacheFilePath, json);

                _cacheData = cacheData;
            }
            catch
            {
                // Cache failures should never interrupt image selection workflows.
            }
        }

        public ImageItem? TryGetValidCache(string filePath, long fileSize, DateTime lastModified)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return null;
                }

                return _cacheData.Images.FirstOrDefault(image =>
                    string.Equals(image.FilePath, filePath, StringComparison.OrdinalIgnoreCase)
                    && image.FileSize == fileSize
                    && image.LastModified.Ticks == lastModified.Ticks
                    && File.Exists(image.FilePath));
            }
            catch
            {
                return null;
            }
        }

        private static string GetCacheFilePath()
        {
            return Path.Combine(AppPathHelper.GetAppDataDirectory(), CacheFileName);
        }

        private static ImageItem CloneForCache(ImageItem image)
        {
            return new ImageItem
            {
                FilePath = image.FilePath,
                FileName = image.FileName,
                Extension = image.Extension,
                FileSize = image.FileSize,
                LastModified = image.LastModified,
                Width = image.Width,
                Height = image.Height,
                ThumbnailPath = image.ThumbnailPath,
                ToneLabel = image.ToneLabel,
                DominantColorHex = image.DominantColorHex,
                IsFavorite = image.IsFavorite,
                Rating = image.Rating,
                IsPicked = image.IsPicked,
                IsRejected = image.IsRejected,
                OrientationLabel = image.OrientationLabel,
                ResolutionLabel = image.ResolutionLabel
            };
        }
    }
}
