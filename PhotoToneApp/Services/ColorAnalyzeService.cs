using PhotoToneApp.Models;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoToneApp.Services
{
    public class ColorAnalyzeService
    {
        private const int AnalyzePixelSize = 64;
        private const int BytesPerPixel = 4;
        private const string UnknownToneLabel = "未知";
        private const string UnknownColorHex = "#000000";

        public ColorAnalyzeResult Analyze(string filePath)
        {
            try
            {
                BitmapImage? image = LoadImageForAnalyze(filePath);
                if (image is null)
                {
                    return CreateUnknownResult();
                }

                FormatConvertedBitmap convertedBitmap = new(image, PixelFormats.Bgra32, null, 0);
                if (convertedBitmap.CanFreeze)
                {
                    convertedBitmap.Freeze();
                }

                return AnalyzePixels(convertedBitmap);
            }
            catch
            {
                return CreateUnknownResult();
            }
        }

        private static BitmapImage? LoadImageForAnalyze(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return null;
                }

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
                    image.DecodePixelWidth = AnalyzePixelSize;
                }
                else
                {
                    image.DecodePixelHeight = AnalyzePixelSize;
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

        private static ColorAnalyzeResult AnalyzePixels(BitmapSource bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            if (width <= 0 || height <= 0)
            {
                return CreateUnknownResult();
            }

            int stride = width * BytesPerPixel;
            byte[] pixels = new byte[stride * height];
            bitmap.CopyPixels(pixels, stride, 0);

            Dictionary<string, ColorBucket> buckets = new(StringComparer.Ordinal)
            {
                ["红色"] = new(),
                ["橙色"] = new(),
                ["黄色"] = new(),
                ["绿色"] = new(),
                ["青色"] = new(),
                ["蓝色"] = new(),
                ["紫色"] = new(),
                ["粉色"] = new(),
                ["棕色"] = new(),
                ["黑色"] = new(),
                ["白色"] = new(),
                ["灰色"] = new()
            };

            ColorBucket allValidPixels = new();
            int validPixelCount = 0;

            for (int index = 0; index < pixels.Length; index += BytesPerPixel)
            {
                byte blue = pixels[index];
                byte green = pixels[index + 1];
                byte red = pixels[index + 2];
                byte alpha = pixels[index + 3];

                if (alpha < 16)
                {
                    continue;
                }

                validPixelCount++;
                (double hue, double saturation, double value) = ConvertRgbToHsv(red, green, blue);
                string toneLabel = ClassifyTone(hue, saturation, value);
                double weight = CalculatePixelWeight(toneLabel, saturation, value);

                buckets[toneLabel].Add(red, green, blue, weight);
                allValidPixels.Add(red, green, blue, weight);
            }

            if (validPixelCount < 16 || allValidPixels.TotalWeight <= 0)
            {
                return CreateUnknownResult();
            }

            KeyValuePair<string, ColorBucket> dominantBucket = buckets
                .Where(bucket => bucket.Value.TotalWeight > 0)
                .OrderByDescending(bucket => bucket.Value.TotalWeight)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(dominantBucket.Key))
            {
                return CreateUnknownResult();
            }

            double dominantRatio = dominantBucket.Value.TotalWeight / allValidPixels.TotalWeight;
            if (dominantRatio < 0.28)
            {
                return new ColorAnalyzeResult
                {
                    ToneLabel = "多色",
                    DominantColorHex = allValidPixels.ToHex()
                };
            }

            return new ColorAnalyzeResult
            {
                ToneLabel = dominantBucket.Key,
                DominantColorHex = dominantBucket.Value.ToHex()
            };
        }

        private static string ClassifyTone(double hue, double saturation, double value)
        {
            if (value < 0.18)
            {
                return "黑色";
            }

            if (saturation < 0.15 && value > 0.82)
            {
                return "白色";
            }

            if (saturation < 0.18)
            {
                return "灰色";
            }

            if (hue >= 15 && hue < 45 && value < 0.65 && saturation >= 0.25)
            {
                return "棕色";
            }

            return hue switch
            {
                >= 0 and < 15 => "红色",
                >= 15 and < 40 => "橙色",
                >= 40 and < 65 => "黄色",
                >= 65 and < 165 => "绿色",
                >= 165 and < 195 => "青色",
                >= 195 and < 255 => "蓝色",
                >= 255 and < 290 => "紫色",
                >= 290 and < 345 => "粉色",
                _ => "红色"
            };
        }

        private static double CalculatePixelWeight(string toneLabel, double saturation, double value)
        {
            if (toneLabel is "黑色" or "白色" or "灰色")
            {
                return 1.0 + (1.0 - saturation) * 0.35;
            }

            return 1.0 + saturation * 1.8 + value * 0.7;
        }

        private static (double Hue, double Saturation, double Value) ConvertRgbToHsv(byte red, byte green, byte blue)
        {
            double r = red / 255d;
            double g = green / 255d;
            double b = blue / 255d;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double hue;
            if (delta == 0)
            {
                hue = 0;
            }
            else if (max == r)
            {
                hue = 60 * (((g - b) / delta) % 6);
            }
            else if (max == g)
            {
                hue = 60 * (((b - r) / delta) + 2);
            }
            else
            {
                hue = 60 * (((r - g) / delta) + 4);
            }

            if (hue < 0)
            {
                hue += 360;
            }

            double saturation = max == 0 ? 0 : delta / max;

            return (hue, saturation, max);
        }

        private static ColorAnalyzeResult CreateUnknownResult()
        {
            return new ColorAnalyzeResult
            {
                ToneLabel = UnknownToneLabel,
                DominantColorHex = UnknownColorHex
            };
        }

        private sealed class ColorBucket
        {
            private double _redTotal;
            private double _greenTotal;
            private double _blueTotal;

            public double TotalWeight { get; private set; }

            public void Add(byte red, byte green, byte blue, double weight)
            {
                _redTotal += red * weight;
                _greenTotal += green * weight;
                _blueTotal += blue * weight;
                TotalWeight += weight;
            }

            public string ToHex()
            {
                if (TotalWeight <= 0)
                {
                    return UnknownColorHex;
                }

                int red = ClampToByte(_redTotal / TotalWeight);
                int green = ClampToByte(_greenTotal / TotalWeight);
                int blue = ClampToByte(_blueTotal / TotalWeight);

                return $"#{red:X2}{green:X2}{blue:X2}";
            }

            private static int ClampToByte(double value)
            {
                return Math.Clamp((int)Math.Round(value), 0, 255);
            }
        }
    }
}
