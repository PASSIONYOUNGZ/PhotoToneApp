using System.IO;

namespace PhotoToneApp.Helpers
{
    public static class AppPathHelper
    {
        public static string GetAppDataDirectory()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataDirectory = Path.Combine(localAppDataPath, "PhotoToneApp");

            Directory.CreateDirectory(appDataDirectory);

            return appDataDirectory;
        }

        public static string GetThumbnailDirectory()
        {
            string thumbnailDirectory = Path.Combine(GetAppDataDirectory(), "Thumbnails");

            Directory.CreateDirectory(thumbnailDirectory);

            return thumbnailDirectory;
        }
    }
}
