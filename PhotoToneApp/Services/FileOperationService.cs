using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace PhotoToneApp.Services
{
    public class FileOperationResult
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public string NewFilePath { get; set; } = string.Empty;
    }

    public class FileOperationService
    {
        public FileOperationResult RenameImage(string filePath, string newNameWithoutExtension)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return Fail("原文件不存在。");
                }

                string trimmedName = newNameWithoutExtension.Trim();
                if (string.IsNullOrWhiteSpace(trimmedName))
                {
                    return Fail("新文件名不能为空。");
                }

                if (trimmedName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return Fail("新文件名包含非法字符。");
                }

                if (!string.IsNullOrEmpty(Path.GetExtension(trimmedName)))
                {
                    return Fail("请输入不含扩展名的文件名。");
                }

                string? directoryPath = Path.GetDirectoryName(filePath);
                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    return Fail("无法获取原文件所在目录。");
                }

                string extension = Path.GetExtension(filePath);
                string newFilePath = Path.Combine(directoryPath, trimmedName + extension);

                if (string.Equals(Path.GetFullPath(filePath), Path.GetFullPath(newFilePath), StringComparison.OrdinalIgnoreCase))
                {
                    return Fail("新文件名与当前文件名相同。");
                }

                if (File.Exists(newFilePath))
                {
                    return Fail("目标文件已存在。");
                }

                File.Move(filePath, newFilePath);

                return new FileOperationResult
                {
                    Success = true,
                    NewFilePath = newFilePath
                };
            }
            catch (Exception ex)
            {
                return Fail($"重命名失败：{ex.Message}");
            }
        }

        public FileOperationResult MoveToRecycleBin(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return Fail("文件不存在。");
                }

                FileSystem.DeleteFile(
                    filePath,
                    UIOption.OnlyErrorDialogs,
                    RecycleOption.SendToRecycleBin);

                return new FileOperationResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return Fail($"移动到回收站失败：{ex.Message}");
            }
        }

        private static FileOperationResult Fail(string errorMessage)
        {
            return new FileOperationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
