namespace PhotoToneApp.Models
{
    public class ImageItem
    {
        public string FilePath { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime LastModified { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string ThumbnailPath { get; set; } = string.Empty;

        public string ToneLabel { get; set; } = "未分析";

        public string DominantColorHex { get; set; } = "#000000";

        public bool IsFavorite { get; set; }

        public int Rating { get; set; } = 0;

        public bool IsPicked { get; set; } = false;

        public bool IsRejected { get; set; } = false;

        public string OrientationLabel { get; set; } = "未知";

        public string ResolutionLabel { get; set; } = "未知";

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
