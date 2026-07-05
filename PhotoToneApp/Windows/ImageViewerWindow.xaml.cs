using PhotoToneApp.Models;
using PhotoToneApp.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PhotoToneApp.Windows
{
    public partial class ImageViewerWindow : Window
    {
        private readonly ImageLoadService _imageLoadService = new();
        private readonly JsonCacheService _jsonCacheService = new();
        private readonly List<ImageItem> _images;
        private readonly List<ImageItem> _cacheImages;
        private int _currentIndex;
        private bool _isFullScreen;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private ResizeMode _previousResizeMode;

        public ImageViewerWindow(List<ImageItem> images, int startIndex)
            : this(images, startIndex, images)
        {
        }

        public ImageViewerWindow(List<ImageItem> images, int startIndex, IEnumerable<ImageItem> cacheImages)
        {
            InitializeComponent();

            _images = images.ToList();
            _cacheImages = cacheImages.ToList();
            _currentIndex = _images.Count == 0 ? -1 : Math.Clamp(startIndex, 0, _images.Count - 1);

            ImageViewer.ViewChanged += ImageViewer_ViewChanged;
            Loaded += ImageViewerWindow_Loaded;
        }

        private void ImageViewerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentImage();
            Focus();
        }

        private void LoadCurrentImage()
        {
            if (_currentIndex < 0 || _images.Count == 0)
            {
                ImageViewer.SetImage(null);
                ViewerMessageTextBlock.Text = "没有可查看的图片";
                ViewerMessageTextBlock.Visibility = Visibility.Visible;
                UpdateInfo(null);
                UpdateNavigationButtons();
                return;
            }

            ImageItem image = _images[_currentIndex];
            ImageViewer.SetImage(_imageLoadService.LoadImage(image.FilePath));
            if (ImageViewer.HasImage)
            {
                ViewerMessageTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ViewerMessageTextBlock.Text = "图片加载失败";
                ViewerMessageTextBlock.Visibility = Visibility.Visible;
            }

            UpdateInfo(image);
            UpdateNavigationButtons();
        }

        private void ShowPreviousImage()
        {
            if (_images.Count == 0)
            {
                return;
            }

            _currentIndex = _currentIndex <= 0 ? _images.Count - 1 : _currentIndex - 1;
            LoadCurrentImage();
        }

        private void ShowNextImage()
        {
            if (_images.Count == 0)
            {
                return;
            }

            _currentIndex = _currentIndex >= _images.Count - 1 ? 0 : _currentIndex + 1;
            LoadCurrentImage();
        }

        private void UpdateInfo(ImageItem? image)
        {
            if (image is null)
            {
                Title = "图片查看";
                HeaderInfoTextBlock.Text = string.Empty;
                FileNameTextBlock.Text = string.Empty;
                IndexTextBlock.Text = "0 / 0";
                DimensionsTextBlock.Text = "尺寸：-";
                ToneTextBlock.Text = "主色调：-";
                FavoriteTextBlock.Text = "收藏：-";
                RatingTextBlock.Text = "评分：-";
                PickStateTextBlock.Text = "入选：-";
                RejectStateTextBlock.Text = "淘汰：-";
                ViewerFavoriteButton.Content = "收藏";
                ViewerPickButton.Content = "入选";
                ViewerRejectButton.Content = "淘汰";
                ZoomTextBlock.Text = "缩放：-";
                return;
            }

            Title = $"图片查看 - {image.FileName}";
            HeaderInfoTextBlock.Text = image.FilePath;
            FileNameTextBlock.Text = image.FileName;
            IndexTextBlock.Text = $"{_currentIndex + 1} / {_images.Count}";
            DimensionsTextBlock.Text = $"尺寸：{image.Width} x {image.Height}";
            ToneTextBlock.Text = $"主色调：{image.ToneLabel} ({image.DominantColorHex})";
            FavoriteTextBlock.Text = image.IsFavorite ? "收藏：已收藏" : "收藏：未收藏";
            RatingTextBlock.Text = $"评分：{FormatRating(image.Rating)}";
            PickStateTextBlock.Text = image.IsPicked ? "入选：是" : "入选：否";
            RejectStateTextBlock.Text = image.IsRejected ? "淘汰：是" : "淘汰：否";
            ViewerFavoriteButton.Content = image.IsFavorite ? "取消收藏" : "收藏";
            ViewerPickButton.Content = image.IsPicked ? "取消入选" : "入选";
            ViewerRejectButton.Content = image.IsRejected ? "取消淘汰" : "淘汰";
            UpdateZoomText();
        }

        private void UpdateNavigationButtons()
        {
            bool hasImages = _images.Count > 0;
            PreviousButton.IsEnabled = hasImages;
            NextButton.IsEnabled = hasImages;
        }

        private void UpdateZoomText()
        {
            ZoomTextBlock.Text = $"缩放：{ImageViewer.CurrentZoom:P0}";
        }

        private void ToggleFullScreen()
        {
            if (_isFullScreen)
            {
                WindowStyle = _previousWindowStyle;
                ResizeMode = _previousResizeMode;
                WindowState = _previousWindowState;
                FullScreenButton.Content = "全屏";
                _isFullScreen = false;
                return;
            }

            _previousWindowState = WindowState;
            _previousWindowStyle = WindowStyle;
            _previousResizeMode = ResizeMode;

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
            FullScreenButton.Content = "退出全屏";
            _isFullScreen = true;
        }

        private ImageItem? GetCurrentImage()
        {
            return _currentIndex >= 0 && _currentIndex < _images.Count
                ? _images[_currentIndex]
                : null;
        }

        private void SetCurrentRating(int rating)
        {
            ImageItem? image = GetCurrentImage();
            if (image is null)
            {
                return;
            }

            image.Rating = Math.Clamp(rating, 0, 5);
            UpdateInfo(image);
            SaveCache();
        }

        private void ToggleCurrentFavorite()
        {
            ImageItem? image = GetCurrentImage();
            if (image is null)
            {
                return;
            }

            image.IsFavorite = !image.IsFavorite;
            UpdateInfo(image);
            SaveCache();
        }

        private void ToggleCurrentPicked()
        {
            ImageItem? image = GetCurrentImage();
            if (image is null)
            {
                return;
            }

            image.IsPicked = !image.IsPicked;
            if (image.IsPicked)
            {
                image.IsRejected = false;
            }

            UpdateInfo(image);
            SaveCache();
        }

        private void ToggleCurrentRejected()
        {
            ImageItem? image = GetCurrentImage();
            if (image is null)
            {
                return;
            }

            image.IsRejected = !image.IsRejected;
            if (image.IsRejected)
            {
                image.IsPicked = false;
            }

            UpdateInfo(image);
            SaveCache();
        }

        private void SaveCache()
        {
            _jsonCacheService.SaveCache(_cacheImages);
        }

        private static string FormatRating(int rating)
        {
            int normalizedRating = Math.Clamp(rating, 0, 5);
            return normalizedRating == 0 ? "未评分" : $"{normalizedRating}星";
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPreviousImage();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNextImage();
        }

        private void FitButton_Click(object sender, RoutedEventArgs e)
        {
            ImageViewer.FitToWindow();
            UpdateZoomText();
        }

        private void ActualSizeButton_Click(object sender, RoutedEventArgs e)
        {
            ImageViewer.SetZoom(1.0);
            UpdateZoomText();
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullScreen();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
            SetCurrentRating(0);
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleCurrentFavorite();
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleCurrentPicked();
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleCurrentRejected();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (TryHandleRatingKey(e.Key))
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Left)
            {
                ShowPreviousImage();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.P)
            {
                ToggleCurrentPicked();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.X)
            {
                ToggleCurrentRejected();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.F)
            {
                ToggleCurrentFavorite();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Right)
            {
                ShowNextImage();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.F11)
            {
                ToggleFullScreen();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                if (_isFullScreen)
                {
                    ToggleFullScreen();
                }
                else
                {
                    Close();
                }

                e.Handled = true;
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

        private void ImageViewer_ViewChanged(object? sender, EventArgs e)
        {
            UpdateZoomText();
        }
    }
}
