using Microsoft.Win32;
using PhotoToneApp.Controls;
using PhotoToneApp.Helpers;
using PhotoToneApp.Models;
using PhotoToneApp.Services;
using PhotoToneApp.Windows;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace PhotoToneApp
{
    public partial class MainWindow : Window
    {
        private readonly ImageScanService _imageScanService = new();
        private readonly ImageLoadService _imageLoadService = new();
        private readonly ThumbnailService _thumbnailService = new();
        private readonly ColorAnalyzeService _colorAnalyzeService = new();
        private readonly FileOperationService _fileOperationService = new();
        private readonly JsonCacheService _jsonCacheService = new();
        private readonly ExportService _exportService = new();
        private readonly List<ImageItem> _allImages = [];
        private ImageItem? _compareLeftImage;
        private ImageItem? _compareRightImage;
        private string _currentFolderPath = string.Empty;
        private string _statusFilter = "全部";
        private string _colorFilter = "全部";
        private string _orientationFilter = "全部";
        private string _resolutionFilter = "全部";
        private string _searchKeyword = string.Empty;
        private string _sortMode = "文件名";
        private string _currentStatus = "就绪";

        public ObservableCollection<ImageItem> VisibleImages { get; } = [];

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadFakeImages();
            ApplyFilters();
            UpdateCompareSelectionText();
        }

        private void LoadFakeImages()
        {
            _allImages.Clear();

            _allImages.AddRange(
            [
                new ImageItem
                {
                    FilePath = @"D:\Photos\Wedding\bride_red_dress.jpg",
                    FileName = "bride_red_dress.jpg",
                    Extension = ".jpg",
                    FileSize = 4_850_000,
                    LastModified = DateTime.Now.AddDays(-8),
                    Width = 6000,
                    Height = 4000,
                    ToneLabel = "红色",
                    DominantColorHex = "#B91C1C",
                    IsFavorite = true
                },
                new ImageItem
                {
                    FilePath = @"D:\Photos\Wedding\sunset_couple.jpg",
                    FileName = "sunset_couple.jpg",
                    Extension = ".jpg",
                    FileSize = 5_420_000,
                    LastModified = DateTime.Now.AddDays(-7),
                    Width = 5472,
                    Height = 3648,
                    ToneLabel = "橙色",
                    DominantColorHex = "#EA580C",
                    IsFavorite = false
                },
                new ImageItem
                {
                    FilePath = @"D:\Photos\Portrait\golden_window.jpg",
                    FileName = "golden_window.jpg",
                    Extension = ".jpg",
                    FileSize = 3_980_000,
                    LastModified = DateTime.Now.AddDays(-6),
                    Width = 4000,
                    Height = 6000,
                    ToneLabel = "黄色",
                    DominantColorHex = "#CA8A04",
                    IsFavorite = true
                },
                new ImageItem
                {
                    FilePath = @"D:\Photos\Travel\forest_path.jpg",
                    FileName = "forest_path.jpg",
                    Extension = ".jpg",
                    FileSize = 6_120_000,
                    LastModified = DateTime.Now.AddDays(-5),
                    Width = 6240,
                    Height = 4160,
                    ToneLabel = "绿色",
                    DominantColorHex = "#15803D",
                    IsFavorite = false
                },
                new ImageItem
                {
                    FilePath = @"D:\Photos\Travel\quiet_lake.jpg",
                    FileName = "quiet_lake.jpg",
                    Extension = ".jpg",
                    FileSize = 4_210_000,
                    LastModified = DateTime.Now.AddDays(-4),
                    Width = 5000,
                    Height = 3333,
                    ToneLabel = "青色",
                    DominantColorHex = "#0891B2",
                    IsFavorite = false
                },
                new ImageItem
                {
                    FilePath = @"D:\Photos\City\blue_hour_street.jpg",
                    FileName = "blue_hour_street.jpg",
                    Extension = ".jpg",
                    FileSize = 7_340_000,
                    LastModified = DateTime.Now.AddDays(-3),
                    Width = 6720,
                    Height = 4480,
                    ToneLabel = "蓝色",
                    DominantColorHex = "#1D4ED8",
                    IsFavorite = true
                },
                new ImageItem
                {
                    FilePath = @"D:\Photos\Portrait\studio_gray_profile.jpg",
                    FileName = "studio_gray_profile.jpg",
                    Extension = ".jpg",
                    FileSize = 2_760_000,
                    LastModified = DateTime.Now.AddDays(-2),
                    Width = 3840,
                    Height = 5760,
                    ToneLabel = "灰色",
                    DominantColorHex = "#6B7280",
                    IsFavorite = false
                },
                new ImageItem
                {
                    FilePath = @"D:\Photos\Event\colorful_stage.jpg",
                    FileName = "colorful_stage.jpg",
                    Extension = ".jpg",
                    FileSize = 8_520_000,
                    LastModified = DateTime.Now.AddDays(-1),
                    Width = 6048,
                    Height = 4024,
                    ToneLabel = "多色",
                    DominantColorHex = "#A855F7",
                    IsFavorite = true
                }
            ]);

            foreach (ImageItem image in _allImages)
            {
                UpdateDimensionLabels(image);
            }

            _currentFolderPath = @"D:\Photos\Sample";
            CurrentFolderTextBlock.Text = $"当前文件夹：{_currentFolderPath}";
        }

        private async Task ScanAndShowFolderAsync(string folderPath)
        {
            SetCurrentStatus("正在读取缓存");
            await Task.Run(() => _jsonCacheService.LoadCache());

            SetCurrentStatus("正在扫描图片");
            List<ImageItem> scannedImages = await Task.Run(() => _imageScanService.ScanFolder(folderPath));

            await PrepareImagesForDisplayAsync(scannedImages);

            _allImages.Clear();
            _allImages.AddRange(scannedImages);

            _currentFolderPath = folderPath;
            CurrentFolderTextBlock.Text = $"当前文件夹：{folderPath}";
            ImageListBox.SelectedItem = null;
            ClearCompareSelection();

            ApplyFilters();
            SaveCurrentCache();

            if (scannedImages.Count == 0)
            {
                SetCurrentStatus("未找到支持格式图片");
                MessageBox.Show("该文件夹内没有支持格式的图片。", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                SetCurrentStatus($"扫描完成：{scannedImages.Count} 张图片");
            }
        }

        private async Task PrepareImagesForDisplayAsync(List<ImageItem> images)
        {
            for (int index = 0; index < images.Count; index++)
            {
                ImageItem image = images[index];
                SetCurrentStatus($"正在处理图片 {index + 1}/{images.Count}");

                try
                {
                    ImageItem? cachedImage = _jsonCacheService.TryGetValidCache(image.FilePath, image.FileSize, image.LastModified);
                    if (cachedImage is not null)
                    {
                        ApplyCachedImageData(image, cachedImage);

                        if (string.IsNullOrWhiteSpace(image.ThumbnailPath) || !File.Exists(image.ThumbnailPath))
                        {
                            image.ThumbnailPath = await Task.Run(() => _thumbnailService.GenerateThumbnail(image.FilePath));
                        }

                        RefreshSelectedImageIfNeeded(image);
                        continue;
                    }

                    (string thumbnailPath, int width, int height, ColorAnalyzeResult analyzeResult) = await Task.Run(() =>
                    {
                        string generatedThumbnailPath = _thumbnailService.GenerateThumbnail(image.FilePath);
                        (int imageWidth, int imageHeight) = _imageLoadService.GetImageSize(image.FilePath);
                        ColorAnalyzeResult colorAnalyzeResult = _colorAnalyzeService.Analyze(image.FilePath);

                        return (generatedThumbnailPath, imageWidth, imageHeight, colorAnalyzeResult);
                    });

                    image.ThumbnailPath = thumbnailPath;
                    image.Width = width;
                    image.Height = height;
                    image.ToneLabel = analyzeResult.ToneLabel;
                    image.DominantColorHex = analyzeResult.DominantColorHex;
                    UpdateDimensionLabels(image);
                    RefreshSelectedImageIfNeeded(image);
                }
                catch
                {
                    image.ToneLabel = "未知";
                    image.DominantColorHex = "#000000";
                    UpdateDimensionLabels(image);
                    RefreshSelectedImageIfNeeded(image);
                }
            }
        }

        private static void ApplyCachedImageData(ImageItem image, ImageItem cachedImage)
        {
            image.Width = cachedImage.Width;
            image.Height = cachedImage.Height;
            image.ToneLabel = string.IsNullOrWhiteSpace(cachedImage.ToneLabel) ? "未知" : cachedImage.ToneLabel;
            image.DominantColorHex = string.IsNullOrWhiteSpace(cachedImage.DominantColorHex)
                ? "#000000"
                : cachedImage.DominantColorHex;
            image.IsFavorite = cachedImage.IsFavorite;
            image.Rating = Math.Clamp(cachedImage.Rating, 0, 5);
            image.IsPicked = cachedImage.IsPicked;
            image.IsRejected = cachedImage.IsRejected;
            image.ThumbnailPath = !string.IsNullOrWhiteSpace(cachedImage.ThumbnailPath) && File.Exists(cachedImage.ThumbnailPath)
                ? cachedImage.ThumbnailPath
                : string.Empty;
            UpdateDimensionLabels(image);
        }

        private static void UpdateDimensionLabels(ImageItem image)
        {
            image.OrientationLabel = ImageDimensionHelper.GetOrientationLabel(image.Width, image.Height);
            image.ResolutionLabel = ImageDimensionHelper.GetResolutionLabel(image.Width, image.Height);
        }

        private void RefreshSelectedImageIfNeeded(ImageItem image)
        {
            if (!ReferenceEquals(ImageListBox.SelectedItem, image))
            {
                return;
            }

            ImageListBox.Items.Refresh();
            UpdateSelectedImageDetails(image);
        }

        private void ApplyFilters(bool selectFirstWhenNoSelection = true)
        {
            IEnumerable<ImageItem> filteredImages = _allImages;

            filteredImages = _statusFilter switch
            {
                "收藏" => filteredImages.Where(image => image.IsFavorite),
                "入选" => filteredImages.Where(image => image.IsPicked),
                "淘汰" => filteredImages.Where(image => image.IsRejected),
                "未评分" => filteredImages.Where(image => image.Rating == 0),
                "3星以上" => filteredImages.Where(image => image.Rating >= 3),
                "4星以上" => filteredImages.Where(image => image.Rating >= 4),
                "5星" => filteredImages.Where(image => image.Rating == 5),
                "全部" => filteredImages,
                _ => filteredImages
            };

            if (_colorFilter != "全部")
            {
                filteredImages = filteredImages.Where(image => image.ToneLabel == _colorFilter);
            }

            filteredImages = _orientationFilter switch
            {
                "横图" => filteredImages.Where(image => image.OrientationLabel is "横图" or "16:9"),
                "竖图" => filteredImages.Where(image => image.OrientationLabel is "竖图" or "9:16"),
                "方图" => filteredImages.Where(image => image.OrientationLabel == "方图"),
                "16:9" => filteredImages.Where(image => image.OrientationLabel == "16:9"),
                "9:16" => filteredImages.Where(image => image.OrientationLabel == "9:16"),
                _ => filteredImages
            };

            filteredImages = _resolutionFilter switch
            {
                "小图" => filteredImages.Where(image => image.ResolutionLabel == "小图"),
                "普通图" => filteredImages.Where(image => image.ResolutionLabel == "普通图"),
                "高清图" => filteredImages.Where(image => image.ResolutionLabel is "高清图" or "大图"),
                "大图" => filteredImages.Where(image => image.ResolutionLabel == "大图"),
                _ => filteredImages
            };

            if (!string.IsNullOrWhiteSpace(_searchKeyword))
            {
                filteredImages = filteredImages.Where(image =>
                    image.FileName.Contains(_searchKeyword, StringComparison.OrdinalIgnoreCase));
            }

            filteredImages = _sortMode switch
            {
                "修改时间" => filteredImages.OrderByDescending(image => image.LastModified),
                "文件大小" => filteredImages.OrderByDescending(image => image.FileSize),
                "宽度" => filteredImages.OrderByDescending(image => image.Width),
                "高度" => filteredImages.OrderByDescending(image => image.Height),
                "评分" => filteredImages
                    .OrderByDescending(image => image.Rating)
                    .ThenBy(image => image.FileName, StringComparer.OrdinalIgnoreCase),
                _ => filteredImages.OrderBy(image => image.FileName, StringComparer.OrdinalIgnoreCase)
            };

            ImageItem? previousSelection = ImageListBox.SelectedItem as ImageItem;

            VisibleImages.Clear();
            foreach (ImageItem image in filteredImages)
            {
                VisibleImages.Add(image);
            }

            if (previousSelection is not null && VisibleImages.Contains(previousSelection))
            {
                ImageListBox.SelectedItem = previousSelection;
            }
            else
            {
                if (selectFirstWhenNoSelection && previousSelection is null && VisibleImages.Count > 0)
                {
                    ImageListBox.SelectedIndex = 0;
                }
                else
                {
                    ImageListBox.SelectedIndex = -1;
                    UpdateSelectedImageDetails(null);
                }
            }

            UpdateStatus();
            UpdateAllFilterButtonVisuals();
        }

        private void UpdateSelectedImageDetails(ImageItem? image)
        {
            if (image is null)
            {
                PreviewImage.Source = null;
                PreviewPlaceholderTextBlock.Text = "大图预览";
                PreviewPlaceholderTextBlock.Visibility = Visibility.Visible;
                DetailFileNameTextBlock.Text = "未选择图片";
                DetailFilePathTextBlock.Text = string.Empty;
                DetailDimensionsTextBlock.Text = string.Empty;
                DetailFileSizeTextBlock.Text = string.Empty;
                DetailToneTextBlock.Text = string.Empty;
                DetailFavoriteTextBlock.Text = string.Empty;
                DetailRatingTextBlock.Text = string.Empty;
                DetailPickedTextBlock.Text = string.Empty;
                DetailRejectedTextBlock.Text = string.Empty;
                DetailOrientationTextBlock.Text = string.Empty;
                DetailResolutionTextBlock.Text = string.Empty;
                DetailDominantColorSwatch.Background = new SolidColorBrush(Colors.Black);
                FavoriteButton.Content = "收藏";
                PickButton.Content = "入选";
                RejectButton.Content = "淘汰";
                SetCurrentStatus("未选择图片");
                return;
            }

            try
            {
                PreviewImage.Source = _imageLoadService.LoadImage(image.FilePath);
            }
            catch
            {
                PreviewImage.Source = null;
            }

            DetailFileNameTextBlock.Text = image.FileName;
            DetailFilePathTextBlock.Text = image.FilePath;
            DetailDimensionsTextBlock.Text = $"{image.Width} x {image.Height}";
            DetailFileSizeTextBlock.Text = FormatFileSize(image.FileSize);
            DetailToneTextBlock.Text = $"{image.ToneLabel} ({image.DominantColorHex})";
            DetailFavoriteTextBlock.Text = image.IsFavorite ? "已收藏" : "未收藏";
            DetailOrientationTextBlock.Text = image.OrientationLabel;
            DetailResolutionTextBlock.Text = image.ResolutionLabel;
            FavoriteButton.Content = image.IsFavorite ? "取消收藏" : "收藏";
            UpdateSelectionMarkInfo();
            DetailDominantColorSwatch.Background = CreateBrushFromHex(image.DominantColorHex);

            if (PreviewImage.Source is null)
            {
                PreviewPlaceholderTextBlock.Text = "预览加载失败";
                PreviewPlaceholderTextBlock.Visibility = Visibility.Visible;
                SetCurrentStatus($"预览加载失败：{image.FileName}");
            }
            else
            {
                PreviewPlaceholderTextBlock.Visibility = Visibility.Collapsed;
                SetCurrentStatus($"已选择：{image.FileName}");
            }
        }

        private void SetCurrentRating(int rating)
        {
            if (ImageListBox.SelectedItem is not ImageItem selectedImage)
            {
                ShowSelectImageMessage();
                return;
            }

            selectedImage.Rating = Math.Clamp(rating, 0, 5);
            RefreshCurrentImageDisplay(selectedImage);
            SaveCurrentCache();
            SetCurrentStatus(selectedImage.Rating == 0
                ? "已清除评分"
                : $"已设置评分：{selectedImage.Rating}星");
        }

        private void ClearCurrentRating()
        {
            SetCurrentRating(0);
        }

        private void ToggleCurrentPicked()
        {
            if (ImageListBox.SelectedItem is not ImageItem selectedImage)
            {
                ShowSelectImageMessage();
                return;
            }

            selectedImage.IsPicked = !selectedImage.IsPicked;
            if (selectedImage.IsPicked)
            {
                selectedImage.IsRejected = false;
            }

            RefreshCurrentImageDisplay(selectedImage);
            SaveCurrentCache();
            SetCurrentStatus(selectedImage.IsPicked
                ? $"已入选：{selectedImage.FileName}"
                : $"已取消入选：{selectedImage.FileName}");
        }

        private void ToggleCurrentRejected()
        {
            if (ImageListBox.SelectedItem is not ImageItem selectedImage)
            {
                ShowSelectImageMessage();
                return;
            }

            selectedImage.IsRejected = !selectedImage.IsRejected;
            if (selectedImage.IsRejected)
            {
                selectedImage.IsPicked = false;
            }

            RefreshCurrentImageDisplay(selectedImage);
            SaveCurrentCache();
            SetCurrentStatus(selectedImage.IsRejected
                ? $"已淘汰：{selectedImage.FileName}"
                : $"已取消淘汰：{selectedImage.FileName}");
        }

        private void UpdateSelectionMarkInfo()
        {
            if (ImageListBox.SelectedItem is not ImageItem selectedImage)
            {
                DetailRatingTextBlock.Text = string.Empty;
                DetailPickedTextBlock.Text = string.Empty;
                DetailRejectedTextBlock.Text = string.Empty;
                PickButton.Content = "入选";
                RejectButton.Content = "淘汰";
                return;
            }

            DetailRatingTextBlock.Text = FormatRating(selectedImage.Rating);
            DetailPickedTextBlock.Text = selectedImage.IsPicked ? "是" : "否";
            DetailRejectedTextBlock.Text = selectedImage.IsRejected ? "是" : "否";
            PickButton.Content = selectedImage.IsPicked ? "取消入选" : "入选";
            RejectButton.Content = selectedImage.IsRejected ? "取消淘汰" : "淘汰";
        }

        private void RefreshCurrentImageDisplay(ImageItem image)
        {
            ImageListBox.Items.Refresh();
            ApplyFilters();

            if (VisibleImages.Contains(image))
            {
                ImageListBox.SelectedItem = image;
                UpdateSelectedImageDetails(image);
            }
            else
            {
                UpdateSelectedImageDetails(null);
            }
        }

        private void ShowSelectImageMessage()
        {
            SetCurrentStatus("请先选择图片");
            MessageBox.Show("请先选择图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static string FormatRating(int rating)
        {
            int normalizedRating = Math.Clamp(rating, 0, 5);
            return normalizedRating == 0 ? "未评分" : $"{normalizedRating}星";
        }

        private void UpdateStatus()
        {
            TotalImagesStatusTextBlock.Text = $"总图片数：{_allImages.Count}";
            VisibleImagesStatusTextBlock.Text = $"当前显示：{VisibleImages.Count}";
            FilterStatusTextBlock.Text = $"筛选：{BuildFilterDescription()}；排序：{_sortMode}";
            SearchStatusTextBlock.Text = string.IsNullOrWhiteSpace(_searchKeyword)
                ? "搜索：无"
                : $"搜索：{_searchKeyword}";
            SelectedImageStatusTextBlock.Text = ImageListBox.SelectedItem is ImageItem selectedImage
                ? $"选中：{selectedImage.FileName}"
                : "选中：无";
            CurrentStatusTextBlock.Text = $"状态：{_currentStatus}";
        }

        private void SetCurrentStatus(string status)
        {
            _currentStatus = status;
            UpdateStatus();
        }

        private void SaveCurrentCache()
        {
            _jsonCacheService.SaveCache(_allImages);
        }

        private string BuildFilterStatus()
        {
            bool hasActiveFilter = _statusFilter != "全部"
                || _colorFilter != "全部"
                || _orientationFilter != "全部"
                || _resolutionFilter != "全部";
            bool hasSearch = !string.IsNullOrWhiteSpace(_searchKeyword);
            bool hasSort = _sortMode != "文件名";

            if (!hasActiveFilter && !hasSearch && !hasSort)
            {
                return "就绪";
            }

            List<string> parts = [];
            if (hasActiveFilter)
            {
                parts.Add($"筛选：{BuildFilterDescription()}");
            }

            if (hasSearch)
            {
                parts.Add($"搜索：{_searchKeyword}");
            }

            if (hasSort)
            {
                parts.Add($"排序：{_sortMode}");
            }

            return string.Join("；", parts);
        }

        private string BuildFilterDescription()
        {
            List<string> filters = [];

            if (_statusFilter != "全部")
            {
                filters.Add(_statusFilter);
            }

            if (_colorFilter != "全部")
            {
                filters.Add(_colorFilter);
            }

            if (_orientationFilter != "全部")
            {
                filters.Add(_orientationFilter);
            }

            if (_resolutionFilter != "全部")
            {
                filters.Add(_resolutionFilter);
            }

            return filters.Count == 0 ? "全部" : string.Join(" + ", filters);
        }

        private void UpdateAllFilterButtonVisuals()
        {
            UpdateFilterButtonVisuals("Status", _statusFilter);
            UpdateFilterButtonVisuals("Color", _colorFilter);
            UpdateFilterButtonVisuals("Orientation", _orientationFilter);
            UpdateFilterButtonVisuals("Resolution", _resolutionFilter);
        }

        private void UpdateFilterButtonVisuals(string groupName, string selectedValue)
        {
            foreach (Button button in FindVisualChildren<Button>(this))
            {
                if (!string.Equals(button.CommandParameter as string, groupName, StringComparison.Ordinal))
                {
                    continue;
                }

                bool isSelected = string.Equals(button.Tag as string, selectedValue, StringComparison.Ordinal);
                if (isSelected)
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(219, 234, 254));
                    button.BorderBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                    button.Foreground = new SolidColorBrush(Color.FromRgb(29, 78, 216));
                    button.BorderThickness = new Thickness(2);
                    button.FontWeight = FontWeights.SemiBold;
                    button.Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(37, 99, 235),
                        BlurRadius = 8,
                        ShadowDepth = 1,
                        Opacity = 0.25
                    };
                }
                else
                {
                    button.ClearValue(Control.BackgroundProperty);
                    button.ClearValue(Control.BorderBrushProperty);
                    button.ClearValue(Control.ForegroundProperty);
                    button.ClearValue(Control.BorderThicknessProperty);
                    button.ClearValue(Control.FontWeightProperty);
                    button.ClearValue(UIElement.EffectProperty);
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
            where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int index = 0; index < childCount; index++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, index);
                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (T descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }

        private static string FormatFileSize(long bytes)
        {
            const double oneMegabyte = 1024d * 1024d;
            return $"{bytes / oneMegabyte:0.00} MB";
        }

        private static Brush CreateBrushFromHex(string colorHex)
        {
            try
            {
                return (Brush)new BrushConverter().ConvertFromString(colorHex)!;
            }
            catch (FormatException)
            {
                return new SolidColorBrush(Colors.Black);
            }
        }

        private static void ShowLaterMessage()
        {
            MessageBox.Show("后续阶段实现", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReleasePreviewImage()
        {
            PreviewImage.Source = null;
            PreviewPlaceholderTextBlock.Text = "大图预览";
            PreviewPlaceholderTextBlock.Visibility = Visibility.Visible;
        }

        private void OpenSelectedImageViewer()
        {
            if (ImageListBox.SelectedItem is not ImageItem selectedImage)
            {
                SetCurrentStatus("请先选择图片");
                MessageBox.Show("请先选择图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            List<ImageItem> images = VisibleImages.ToList();
            if (images.Count == 0)
            {
                SetCurrentStatus("当前没有可查看的图片");
                MessageBox.Show("当前没有可查看的图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int startIndex = images.IndexOf(selectedImage);
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            ImageViewerWindow viewerWindow = new(images, startIndex, _allImages)
            {
                Owner = this
            };
            viewerWindow.Closed += ImageViewerWindow_Closed;
            viewerWindow.Show();
            SetCurrentStatus($"打开大图查看：{images[startIndex].FileName}");
        }

        private void ExportImages()
        {
            string exportMode = GetSelectedExportMode();
            List<ImageItem> images = GetImagesForExport(exportMode);

            if (images.Count == 0)
            {
                SetCurrentStatus("没有可导出的图片");
                MessageBox.Show("没有可导出的图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OpenFolderDialog dialog = new()
            {
                Title = "选择导出目标文件夹",
                Multiselect = false
            };

            if (!string.IsNullOrWhiteSpace(_currentFolderPath) && Directory.Exists(_currentFolderPath))
            {
                dialog.InitialDirectory = _currentFolderPath;
            }

            bool? dialogResult = dialog.ShowDialog(this);
            if (dialogResult != true)
            {
                return;
            }

            SetCurrentStatus("正在导出...");
            ExportResult result = _exportService.CopyImagesToFolder(images, dialog.FolderName);

            string message = BuildExportResultMessage(exportMode, result, dialog.FolderName);
            MessageBox.Show(
                message,
                "导出完成",
                MessageBoxButton.OK,
                result.FailedCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

            SetCurrentStatus(result.FailedCount > 0
                ? $"导出完成，部分失败：成功 {result.SuccessCount} / 总数 {result.TotalCount}"
                : $"导出完成：成功 {result.SuccessCount} / 总数 {result.TotalCount}");
        }

        private string GetSelectedExportMode()
        {
            return ExportModeComboBox.SelectedItem is ComboBoxItem item && item.Content is string exportMode
                ? exportMode
                : "导出当前显示";
        }

        private List<ImageItem> GetImagesForExport(string exportMode)
        {
            return exportMode switch
            {
                "导出收藏" => _allImages.Where(image => image.IsFavorite).ToList(),
                "导出入选" => _allImages.Where(image => image.IsPicked).ToList(),
                "导出3星以上" => _allImages.Where(image => image.Rating >= 3).ToList(),
                "导出5星" => _allImages.Where(image => image.Rating == 5).ToList(),
                _ => VisibleImages.ToList()
            };
        }

        private static string BuildExportResultMessage(string exportMode, ExportResult result, string targetFolder)
        {
            List<string> lines =
            [
                $"导出模式：{exportMode}",
                $"总数：{result.TotalCount}",
                $"成功：{result.SuccessCount}",
                $"失败：{result.FailedCount}",
                $"目标文件夹：{targetFolder}"
            ];

            if (result.FailedCount > 0 && result.FailedFiles.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add("失败文件：");
                lines.AddRange(result.FailedFiles.Take(8));

                if (result.FailedFiles.Count > 8)
                {
                    lines.Add($"还有 {result.FailedFiles.Count - 8} 个失败文件未显示");
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private void ImageViewerWindow_Closed(object? sender, EventArgs e)
        {
            ImageListBox.Items.Refresh();
            ApplyFilters();

            if (ImageListBox.SelectedItem is ImageItem selectedImage)
            {
                UpdateSelectedImageDetails(selectedImage);
            }
            else
            {
                UpdateSelectedImageDetails(null);
            }

            SetCurrentStatus("大图查看已关闭");
        }

        private void OpenCompareWindow()
        {
            List<ImageItem> images = VisibleImages.ToList();
            if (images.Count < 2)
            {
                SetCurrentStatus("至少需要两张图片才能对比");
                MessageBox.Show("至少需要两张图片才能对比", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_compareLeftImage is null || _compareRightImage is null)
            {
                SetCurrentStatus("请先设置左图和右图");
                MessageBox.Show("请先设置左图和右图", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ReferenceEquals(_compareLeftImage, _compareRightImage)
                || string.Equals(_compareLeftImage.FilePath, _compareRightImage.FilePath, StringComparison.OrdinalIgnoreCase))
            {
                SetCurrentStatus("左图和右图不能是同一张图片");
                MessageBox.Show("左图和右图不能是同一张图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int leftIndex = images.IndexOf(_compareLeftImage);
            int rightIndex = images.IndexOf(_compareRightImage);
            if (leftIndex < 0)
            {
                SetCurrentStatus("对比图片不在当前筛选结果中");
                MessageBox.Show("对比图片不在当前筛选结果中，请重新选择", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (rightIndex < 0)
            {
                SetCurrentStatus("对比图片不在当前筛选结果中");
                MessageBox.Show("对比图片不在当前筛选结果中，请重新选择", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ImageCompareWindow compareWindow = new(images, leftIndex, rightIndex, _allImages)
            {
                Owner = this
            };
            compareWindow.Closed += ImageCompareWindow_Closed;
            compareWindow.Show();
            SetCurrentStatus($"打开双图对比：{images[leftIndex].FileName} / {images[rightIndex].FileName}");
        }

        private void ImageCompareWindow_Closed(object? sender, EventArgs e)
        {
            ImageListBox.Items.Refresh();
            ApplyFilters();

            if (ImageListBox.SelectedItem is ImageItem selectedImage)
            {
                UpdateSelectedImageDetails(selectedImage);
            }
            else
            {
                UpdateSelectedImageDetails(null);
            }

            SetCurrentStatus("双图对比已关闭");
        }

        private void SetCompareImage(bool isLeft)
        {
            if (ImageListBox.SelectedItem is not ImageItem selectedImage)
            {
                SetCurrentStatus("请先选择图片");
                MessageBox.Show("请先选择图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (isLeft)
            {
                _compareLeftImage = selectedImage;
                SetCurrentStatus($"已设为左图：{selectedImage.FileName}");
            }
            else
            {
                _compareRightImage = selectedImage;
                SetCurrentStatus($"已设为右图：{selectedImage.FileName}");
            }

            UpdateCompareSelectionText();
        }

        private void ClearCompareSelection()
        {
            _compareLeftImage = null;
            _compareRightImage = null;
            UpdateCompareSelectionText();
        }

        private void UpdateCompareSelectionText()
        {
            CompareLeftText.Text = _compareLeftImage is null
                ? "左图：未选择"
                : $"左图：{_compareLeftImage.FileName}";
            CompareRightText.Text = _compareRightImage is null
                ? "右图：未选择"
                : $"右图：{_compareRightImage.FileName}";
        }

        private void ClearDeletedCompareSelection(ImageItem deletedImage)
        {
            if (ReferenceEquals(_compareLeftImage, deletedImage))
            {
                _compareLeftImage = null;
            }

            if (ReferenceEquals(_compareRightImage, deletedImage))
            {
                _compareRightImage = null;
            }

            UpdateCompareSelectionText();
        }

        private void UpdateImageItemAfterRename(ImageItem image, string newFilePath)
        {
            FileInfo fileInfo = new(newFilePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("重命名后的文件不存在。", newFilePath);
            }

            image.FilePath = fileInfo.FullName;
            image.FileName = fileInfo.Name;
            image.Extension = fileInfo.Extension;
            image.FileSize = fileInfo.Length;
            image.LastModified = fileInfo.LastWriteTime;

            try
            {
                image.ThumbnailPath = _thumbnailService.GenerateThumbnail(image.FilePath);
            }
            catch
            {
                image.ThumbnailPath = string.Empty;
            }

            try
            {
                (int width, int height) = _imageLoadService.GetImageSize(image.FilePath);
                image.Width = width;
                image.Height = height;
            }
            catch
            {
                image.Width = 0;
                image.Height = 0;
            }

            UpdateDimensionLabels(image);
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new()
            {
                Title = "选择图片文件夹",
                Multiselect = false
            };

            if (!string.IsNullOrWhiteSpace(_currentFolderPath) && Directory.Exists(_currentFolderPath))
            {
                dialog.InitialDirectory = _currentFolderPath;
            }

            bool? result = dialog.ShowDialog(this);
            if (result != true)
            {
                return;
            }

            OpenFolderButton.IsEnabled = false;
            try
            {
                await ScanAndShowFolderAsync(dialog.FolderName);
            }
            finally
            {
                OpenFolderButton.IsEnabled = true;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLaterMessage();
        }

        private void OpenViewerButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedImageViewer();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            ExportImages();
        }

        private void RatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int rating))
            {
                SetCurrentRating(rating);
            }
        }

        private void ClearRatingButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCurrentRating();
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleCurrentPicked();
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleCurrentRejected();
        }

        private void SetCompareLeftButton_Click(object sender, RoutedEventArgs e)
        {
            SetCompareImage(isLeft: true);
        }

        private void SetCompareRightButton_Click(object sender, RoutedEventArgs e)
        {
            SetCompareImage(isLeft: false);
        }

        private void ClearCompareButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCompareSelection();
            SetCurrentStatus("已清空对比选择");
        }

        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            OpenCompareWindow();
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListBox.SelectedItem is not ImageItem selectedImage)
            {
                SetCurrentStatus("请先选择图片");
                MessageBox.Show("请先选择图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            selectedImage.IsFavorite = !selectedImage.IsFavorite;
            string status = selectedImage.IsFavorite
                ? $"已收藏：{selectedImage.FileName}"
                : $"已取消收藏：{selectedImage.FileName}";

            ImageListBox.Items.Refresh();
            ApplyFilters();

            if (VisibleImages.Contains(selectedImage))
            {
                ImageListBox.SelectedItem = selectedImage;
                UpdateSelectedImageDetails(selectedImage);
            }
            else
            {
                UpdateSelectedImageDetails(null);
            }

            SaveCurrentCache();
            SetCurrentStatus(status);
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListBox.SelectedItem is not ImageItem selectedImage)
            {
                SetCurrentStatus("请先选择图片");
                MessageBox.Show("请先选择图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            RenameDialog dialog = new(Path.GetFileNameWithoutExtension(selectedImage.FileName))
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            ReleasePreviewImage();

            FileOperationResult result = _fileOperationService.RenameImage(selectedImage.FilePath, dialog.NewFileName);
            if (!result.Success)
            {
                MessageBox.Show(result.ErrorMessage, "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Warning);
                UpdateSelectedImageDetails(selectedImage);
                SetCurrentStatus("重命名失败");
                return;
            }

            try
            {
                UpdateImageItemAfterRename(selectedImage, result.NewFilePath);
                UpdateCompareSelectionText();
                ImageListBox.Items.Refresh();
                ApplyFilters();
                SaveCurrentCache();

                if (VisibleImages.Contains(selectedImage))
                {
                    ImageListBox.SelectedItem = selectedImage;
                    UpdateSelectedImageDetails(selectedImage);
                }
                else
                {
                    UpdateSelectedImageDetails(null);
                }

                SetCurrentStatus("重命名成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"文件已重命名，但刷新界面失败：{ex.Message}", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Warning);
                ApplyFilters();
                SaveCurrentCache();
                SetCurrentStatus("重命名成功，界面刷新失败");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListBox.SelectedItem is not ImageItem selectedImage)
            {
                SetCurrentStatus("请先选择图片");
                MessageBox.Show("请先选择图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult confirmResult = MessageBox.Show(
                $"是否将该图片移动到回收站？\n文件名：{selectedImage.FileName}",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }

            string deletedFileName = selectedImage.FileName;
            ReleasePreviewImage();

            FileOperationResult result = _fileOperationService.MoveToRecycleBin(selectedImage.FilePath);
            if (!result.Success)
            {
                MessageBox.Show(result.ErrorMessage, "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Warning);
                UpdateSelectedImageDetails(selectedImage);
                SetCurrentStatus("删除失败");
                return;
            }

            _allImages.Remove(selectedImage);
            ClearDeletedCompareSelection(selectedImage);
            ImageListBox.SelectedItem = null;
            ApplyFilters(selectFirstWhenNoSelection: false);
            SaveCurrentCache();
            UpdateSelectedImageDetails(null);
            SetCurrentStatus($"已移动到回收站：{deletedFileName}");
        }

        private void StatusFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string statusFilter)
            {
                _statusFilter = statusFilter;
                ApplyFilters();
                SetCurrentStatus(BuildFilterStatus());
            }
        }

        private void ColorFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string colorFilter)
            {
                _colorFilter = colorFilter;
                ApplyFilters();
                SetCurrentStatus(BuildFilterStatus());
            }
        }

        private void OrientationFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string orientationFilter)
            {
                _orientationFilter = orientationFilter;
                ApplyFilters();
                SetCurrentStatus(BuildFilterStatus());
            }
        }

        private void ResolutionFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string resolutionFilter)
            {
                _resolutionFilter = resolutionFilter;
                ApplyFilters();
                SetCurrentStatus(BuildFilterStatus());
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox?.SelectedItem is ComboBoxItem item && item.Content is string sortMode)
            {
                _sortMode = sortMode;
            }

            if (ImageListBox is null)
            {
                return;
            }

            ApplyFilters();
            SetCurrentStatus(BuildFilterStatus());
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchKeyword = SearchTextBox.Text.Trim();
            ApplyFilters();
            SetCurrentStatus(BuildFilterStatus());
        }

        private void ImageListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ImageListBox.SelectedItem is ImageItem)
            {
                OpenSelectedImageViewer();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is TextBox || Keyboard.FocusedElement is TextBox)
            {
                return;
            }

            if (TryHandleRatingKey(e.Key))
            {
                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.P:
                    ToggleCurrentPicked();
                    e.Handled = true;
                    break;
                case Key.X:
                    ToggleCurrentRejected();
                    e.Handled = true;
                    break;
                case Key.F:
                    FavoriteButton_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.Delete:
                    DeleteButton_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    OpenSelectedImageViewer();
                    e.Handled = true;
                    break;
                case Key.C:
                    OpenCompareWindow();
                    e.Handled = true;
                    break;
                case Key.Left:
                    SelectAdjacentVisibleImage(-1);
                    e.Handled = true;
                    break;
                case Key.Right:
                    SelectAdjacentVisibleImage(1);
                    e.Handled = true;
                    break;
            }
        }

        private bool TryHandleRatingKey(Key key)
        {
            int rating = key switch
            {
                Key.D0 or Key.NumPad0 => 0,
                Key.D1 or Key.NumPad1 => 1,
                Key.D2 or Key.NumPad2 => 2,
                Key.D3 or Key.NumPad3 => 3,
                Key.D4 or Key.NumPad4 => 4,
                Key.D5 or Key.NumPad5 => 5,
                _ => -1
            };

            if (rating < 0)
            {
                return false;
            }

            SetCurrentRating(rating);
            return true;
        }

        private void SelectAdjacentVisibleImage(int delta)
        {
            if (VisibleImages.Count == 0)
            {
                return;
            }

            int currentIndex = ImageListBox.SelectedItem is ImageItem selectedImage
                ? VisibleImages.IndexOf(selectedImage)
                : -1;

            int nextIndex = currentIndex < 0
                ? 0
                : Math.Clamp(currentIndex + delta, 0, VisibleImages.Count - 1);

            ImageListBox.SelectedIndex = nextIndex;
            ImageListBox.ScrollIntoView(VisibleImages[nextIndex]);
        }

        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedImageDetails(ImageListBox.SelectedItem as ImageItem);
        }
    }
}
