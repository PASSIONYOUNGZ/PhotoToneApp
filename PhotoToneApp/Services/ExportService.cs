using PhotoToneApp.Models;
using System.IO;

namespace PhotoToneApp.Services
{
    public class ExportResult
    {
        public int TotalCount { get; set; }

        public int SuccessCount { get; set; }

        public int FailedCount { get; set; }

        public List<string> FailedFiles { get; set; } = [];

        public string Message { get; set; } = string.Empty;
    }

    public class ExportService
    {
        public ExportResult CopyImagesToFolder(IEnumerable<ImageItem> images, string targetFolder)
        {
            ExportResult result = new();

            try
            {
                List<ImageItem> imageList = images?.ToList() ?? [];
                result.TotalCount = imageList.Count;

                if (imageList.Count == 0)
                {
                    result.Message = "没有可导出的图片";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(targetFolder))
                {
                    result.FailedCount = imageList.Count;
                    result.FailedFiles.Add("目标文件夹为空");
                    result.Message = "目标文件夹为空";
                    return result;
                }

                Directory.CreateDirectory(targetFolder);

                foreach (ImageItem image in imageList)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(image.FilePath) || !File.Exists(image.FilePath))
                        {
                            result.FailedFiles.Add($"{image.FileName}：原图不存在");
                            continue;
                        }

                        string fileName = string.IsNullOrWhiteSpace(image.FileName)
                            ? Path.GetFileName(image.FilePath)
                            : image.FileName;
                        string targetPath = GetUniqueTargetPath(targetFolder, fileName);

                        File.Copy(image.FilePath, targetPath, overwrite: false);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        string failedName = string.IsNullOrWhiteSpace(image.FileName)
                            ? image.FilePath
                            : image.FileName;
                        result.FailedFiles.Add($"{failedName}：{ex.Message}");
                    }
                }

                result.FailedCount = result.TotalCount - result.SuccessCount;
                result.Message = result.FailedCount == 0
                    ? "导出完成"
                    : "导出完成，部分失败";
            }
            catch (Exception ex)
            {
                result.Message = $"导出失败：{ex.Message}";
                result.FailedCount = result.TotalCount - result.SuccessCount;
                result.FailedFiles.Add(ex.Message);
            }

            return result;
        }

        private string GetUniqueTargetPath(string targetFolder, string fileName)
        {
            string safeFileName = string.IsNullOrWhiteSpace(fileName) ? "image" : fileName;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(safeFileName);
            string extension = Path.GetExtension(safeFileName);

            if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            {
                fileNameWithoutExtension = "image";
            }

            string targetPath = Path.Combine(targetFolder, $"{fileNameWithoutExtension}{extension}");
            if (!File.Exists(targetPath))
            {
                return targetPath;
            }

            for (int index = 1; index <= 9999; index++)
            {
                targetPath = Path.Combine(targetFolder, $"{fileNameWithoutExtension}_{index}{extension}");
                if (!File.Exists(targetPath))
                {
                    return targetPath;
                }
            }

            throw new IOException($"无法生成唯一文件名：{safeFileName}");
        }
    }
}
