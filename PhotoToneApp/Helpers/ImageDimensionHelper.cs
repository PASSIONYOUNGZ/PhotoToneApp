namespace PhotoToneApp.Helpers
{
    public static class ImageDimensionHelper
    {
        public static string GetOrientationLabel(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return "未知";
            }

            if (Math.Abs(width - height) <= Math.Min(width, height) * 0.1)
            {
                return "方图";
            }

            if (IsWide16To9(width, height))
            {
                return "16:9";
            }

            if (IsVertical9To16(width, height))
            {
                return "9:16";
            }

            return width > height ? "横图" : "竖图";
        }

        public static string GetResolutionLabel(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return "未知";
            }

            long pixels = (long)width * height;
            int maxSide = Math.Max(width, height);
            int minSide = Math.Min(width, height);

            if (maxSide < 1000 || pixels < 1_000_000)
            {
                return "小图";
            }

            if (maxSide >= 3000 || pixels >= 8_000_000)
            {
                return "大图";
            }

            if ((maxSide >= 1920 && minSide >= 1080) || pixels >= 2_000_000)
            {
                return "高清图";
            }

            return "普通图";
        }

        public static bool IsWide16To9(int width, int height)
        {
            if (width <= 0 || height <= 0 || width <= height)
            {
                return false;
            }

            double ratio = (double)width / height;
            return ratio is >= 1.70 and <= 1.85;
        }

        public static bool IsVertical9To16(int width, int height)
        {
            if (width <= 0 || height <= 0 || height <= width)
            {
                return false;
            }

            double ratio = (double)width / height;
            return ratio is >= 0.54 and <= 0.62;
        }
    }
}
