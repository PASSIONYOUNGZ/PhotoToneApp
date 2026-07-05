using PhotoToneApp.Controls;
using PhotoToneApp.Models;
using PhotoToneApp.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PhotoToneApp.Windows
{
    public partial class ImageCompareWindow : Window
    {
        private readonly ImageLoadService _imageLoadService = new();
        private readonly JsonCacheService _jsonCacheService = new();
        private static readonly string[] RatingOptions = ["未评分", "1星", "2星", "3星", "4星", "5星"];
        private readonly List<ImageItem> _images;
        private readonly List<ImageItem> _cacheImages;
        private int _leftIndex;
        private int _rightIndex;
        private bool _isSyncingView;
        private bool _isUpdatingSelection;
        private bool _isUpdatingRatingControls;
        private CompareSide _activeSide = CompareSide.Left;

        private enum CompareSide
        {
            Left,
            Right
        }

        public ImageCompareWindow(List<ImageItem> images, int leftIndex, int rightIndex)
            : this(images, leftIndex, rightIndex, images)
        {
        }

        public ImageCompareWindow(List<ImageItem> images, int leftIndex, int rightIndex, IEnumerable<ImageItem> cacheImages)
        {
            InitializeComponent();

            _images = images.ToList();
            _cacheImages = cacheImages.ToList();
            _leftIndex = NormalizeIndex(leftIndex);
            _rightIndex = NormalizeIndex(rightIndex);

            InitializeImageSelectors();
            InitializeRatingSelectors();

            LeftViewer.ViewChanged += LeftViewer_ViewChanged;
            RightViewer.ViewChanged += RightViewer_ViewChanged;
            Loaded += ImageCompareWindow_Loaded;
        }

        private void InitializeImageSelectors()
        {
            _isUpdatingSelection = true;
            try
            {
                LeftImageComboBox.DisplayMemberPath = nameof(ImageItem.FileName);
                RightImageComboBox.DisplayMemberPath = nameof(ImageItem.FileName);
                LeftImageComboBox.ItemsSource = _images;
                RightImageComboBox.ItemsSource = _images;
                LeftImageComboBox.SelectedIndex = _leftIndex;
                RightImageComboBox.SelectedIndex = _rightIndex;
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }

        private void InitializeRatingSelectors()
        {
            _isUpdatingRatingControls = true;
            try
            {
                LeftRatingComboBox.ItemsSource = RatingOptions;
                RightRatingComboBox.ItemsSource = RatingOptions;
            }
            finally
            {
                _isUpdatingRatingControls = false;
            }
        }

        private void ImageCompareWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLeftImage();
            LoadRightImage();
            Focus();
        }

        private int NormalizeIndex(int index)
        {
            if (_images.Count == 0)
            {
                return -1;
            }

            return Math.Clamp(index, 0, _images.Count - 1);
        }

        private void LoadLeftImage()
        {
            LoadImageSide(LeftViewer, LeftMessageTextBlock, LeftFileNameTextBlock, LeftInfoTextBlock, FavoriteLeftButton, _leftIndex, "左图");
            UpdateSideMarkControls(_leftIndex, LeftRatingComboBox, LeftPickButton, LeftRejectButton);
        }

        private void LoadRightImage()
        {
            LoadImageSide(RightViewer, RightMessageTextBlock, RightFileNameTextBlock, RightInfoTextBlock, FavoriteRightButton, _rightIndex, "右图");
            UpdateSideMarkControls(_rightIndex, RightRatingComboBox, RightPickButton, RightRejectButton);
        }

        private void LoadImageSide(
            ZoomableImageViewer viewer,
            TextBlock messageTextBlock,
            TextBlock fileNameTextBlock,
            TextBlock infoTextBlock,
            Button favoriteButton,
            int index,
            string sideLabel)
        {
            if (index < 0 || index >= _images.Count)
            {
                viewer.SetImage(null);
                messageTextBlock.Text = "没有可查看的图片";
                messageTextBlock.Visibility = Visibility.Visible;
                fileNameTextBlock.Text = $"{sideLabel}：无";
                infoTextBlock.Text = string.Empty;
                favoriteButton.Content = $"收藏{sideLabel}";
                return;
            }

            ImageItem image = _images[index];
            viewer.SetImage(_imageLoadService.LoadImage(image.FilePath));
            messageTextBlock.Visibility = viewer.HasImage ? Visibility.Collapsed : Visibility.Visible;
            messageTextBlock.Text = viewer.HasImage ? string.Empty : "图片加载失败";

            fileNameTextBlock.Text = $"{sideLabel}：{image.FileName}";
            infoTextBlock.Text = BuildImageInfo(image, index);
            favoriteButton.Content = image.IsFavorite ? $"取消收藏{sideLabel}" : $"收藏{sideLabel}";
        }

        private string BuildImageInfo(ImageItem image, int index)
        {
            string favoriteText = image.IsFavorite ? "已收藏" : "未收藏";
            string pickedText = image.IsPicked ? "是" : "否";
            string rejectedText = image.IsRejected ? "是" : "否";
            return $"{index + 1} / {_images.Count}    {image.Width} x {image.Height}    主色调：{image.ToneLabel} ({image.DominantColorHex})    收藏：{favoriteText}    评分：{FormatRating(image.Rating)}    入选：{pickedText}    淘汰：{rejectedText}";
        }

        private void UpdateSideMarkControls(int index, ComboBox ratingComboBox, Button pickButton, Button rejectButton)
        {
            _isUpdatingRatingControls = true;
            try
            {
                if (index < 0 || index >= _images.Count)
                {
                    ratingComboBox.SelectedIndex = 0;
                    pickButton.Content = "入选";
                    rejectButton.Content = "淘汰";
                    return;
                }

                ImageItem image = _images[index];
                ratingComboBox.SelectedIndex = Math.Clamp(image.Rating, 0, 5);
                pickButton.Content = image.IsPicked ? "取消入选" : "入选";
                rejectButton.Content = image.IsRejected ? "取消淘汰" : "淘汰";
            }
            finally
            {
                _isUpdatingRatingControls = false;
            }
        }

        private static string FormatRating(int rating)
        {
            int normalizedRating = Math.Clamp(rating, 0, 5);
            return normalizedRating == 0 ? "未评分" : $"{normalizedRating}星";
        }

        private void MoveLeft(int delta)
        {
            if (_images.Count == 0)
            {
                return;
            }

            _leftIndex = GetNextDistinctIndex(_leftIndex, _rightIndex, delta);
            UpdateComboBoxSelection(LeftImageComboBox, _leftIndex);
            LoadLeftImage();
            SetActiveSide(CompareSide.Left);
        }

        private void MoveRight(int delta)
        {
            if (_images.Count == 0)
            {
                return;
            }

            _rightIndex = GetNextDistinctIndex(_rightIndex, _leftIndex, delta);
            UpdateComboBoxSelection(RightImageComboBox, _rightIndex);
            LoadRightImage();
            SetActiveSide(CompareSide.Right);
        }

        private void UpdateComboBoxSelection(ComboBox comboBox, int index)
        {
            _isUpdatingSelection = true;
            try
            {
                comboBox.SelectedIndex = index;
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }

        private void RestoreComboBoxSelection(ComboBox comboBox, int index)
        {
            UpdateComboBoxSelection(comboBox, index);
        }

        private int WrapIndex(int index)
        {
            if (_images.Count == 0)
            {
                return -1;
            }

            if (index < 0)
            {
                return _images.Count - 1;
            }

            if (index >= _images.Count)
            {
                return 0;
            }

            return index;
        }

        private int GetNextDistinctIndex(int currentIndex, int otherIndex, int delta)
        {
            int nextIndex = WrapIndex(currentIndex + delta);
            if (_images.Count > 1 && nextIndex == otherIndex)
            {
                nextIndex = WrapIndex(nextIndex + delta);
            }

            return nextIndex;
        }

        private bool IsSyncEnabled()
        {
            return SyncViewCheckBox.IsChecked == true;
        }

        private void SyncView(ZoomableImageViewer source, ZoomableImageViewer target)
        {
            if (_isSyncingView || !IsSyncEnabled() || !source.HasImage || !target.HasImage)
            {
                return;
            }

            try
            {
                _isSyncingView = true;
                target.ApplyView(source.CurrentZoom, source.OffsetX, source.OffsetY, raiseEvent: false);
            }
            finally
            {
                _isSyncingView = false;
            }
        }

        private void ToggleFavorite(int index, bool isLeft)
        {
            if (index < 0 || index >= _images.Count)
            {
                return;
            }

            ImageItem image = _images[index];
            image.IsFavorite = !image.IsFavorite;

            if (isLeft)
            {
                RefreshLeftInfo();
            }
            else
            {
                RefreshRightInfo();
            }

            _jsonCacheService.SaveCache(_cacheImages);
        }

        private void SetRating(int index, int rating, bool isLeft)
        {
            if (index < 0 || index >= _images.Count)
            {
                return;
            }

            _images[index].Rating = Math.Clamp(rating, 0, 5);
            RefreshSideInfo(isLeft);
            _jsonCacheService.SaveCache(_cacheImages);
        }

        private void TogglePicked(int index, bool isLeft)
        {
            if (index < 0 || index >= _images.Count)
            {
                return;
            }

            ImageItem image = _images[index];
            image.IsPicked = !image.IsPicked;
            if (image.IsPicked)
            {
                image.IsRejected = false;
            }

            RefreshSideInfo(isLeft);
            _jsonCacheService.SaveCache(_cacheImages);
        }

        private void ToggleRejected(int index, bool isLeft)
        {
            if (index < 0 || index >= _images.Count)
            {
                return;
            }

            ImageItem image = _images[index];
            image.IsRejected = !image.IsRejected;
            if (image.IsRejected)
            {
                image.IsPicked = false;
            }

            RefreshSideInfo(isLeft);
            _jsonCacheService.SaveCache(_cacheImages);
        }

        private void RefreshSideInfo(bool isLeft)
        {
            if (isLeft)
            {
                RefreshLeftInfo();
            }
            else
            {
                RefreshRightInfo();
            }
        }

        private void RefreshLeftInfo()
        {
            RefreshSideInfo(LeftFileNameTextBlock, LeftInfoTextBlock, FavoriteLeftButton, LeftRatingComboBox, LeftPickButton, LeftRejectButton, _leftIndex, "左图");
        }

        private void RefreshRightInfo()
        {
            RefreshSideInfo(RightFileNameTextBlock, RightInfoTextBlock, FavoriteRightButton, RightRatingComboBox, RightPickButton, RightRejectButton, _rightIndex, "右图");
        }

        private void RefreshSideInfo(
            TextBlock fileNameTextBlock,
            TextBlock infoTextBlock,
            Button favoriteButton,
            ComboBox ratingComboBox,
            Button pickButton,
            Button rejectButton,
            int index,
            string sideLabel)
        {
            if (index < 0 || index >= _images.Count)
            {
                fileNameTextBlock.Text = $"{sideLabel}：无";
                infoTextBlock.Text = string.Empty;
                favoriteButton.Content = $"收藏{sideLabel}";
                UpdateSideMarkControls(index, ratingComboBox, pickButton, rejectButton);
                return;
            }

            ImageItem image = _images[index];
            fileNameTextBlock.Text = $"{sideLabel}：{image.FileName}";
            infoTextBlock.Text = BuildImageInfo(image, index);
            favoriteButton.Content = image.IsFavorite ? $"取消收藏{sideLabel}" : $"收藏{sideLabel}";
            UpdateSideMarkControls(index, ratingComboBox, pickButton, rejectButton);
        }

        private void ReplaceLeftImage(int newIndex)
        {
            if (!CanUseNewIndex(newIndex))
            {
                RestoreComboBoxSelection(LeftImageComboBox, _leftIndex);
                return;
            }

            if (newIndex == _rightIndex)
            {
                MessageBox.Show("左右不能选择同一张图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                RestoreComboBoxSelection(LeftImageComboBox, _leftIndex);
                return;
            }

            _leftIndex = newIndex;
            LoadLeftImage();
            SetActiveSide(CompareSide.Left);

            if (IsSyncEnabled())
            {
                RightViewer.FitToWindow();
            }
        }

        private void ReplaceRightImage(int newIndex)
        {
            if (!CanUseNewIndex(newIndex))
            {
                RestoreComboBoxSelection(RightImageComboBox, _rightIndex);
                return;
            }

            if (newIndex == _leftIndex)
            {
                MessageBox.Show("左右不能选择同一张图片", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                RestoreComboBoxSelection(RightImageComboBox, _rightIndex);
                return;
            }

            _rightIndex = newIndex;
            LoadRightImage();
            SetActiveSide(CompareSide.Right);

            if (IsSyncEnabled())
            {
                LeftViewer.FitToWindow();
            }
        }

        private bool CanUseNewIndex(int index)
        {
            return index >= 0 && index < _images.Count;
        }

        private void FitBoth()
        {
            LeftViewer.FitToWindow();
            RightViewer.FitToWindow();
        }

        private void SetBothActualSize()
        {
            LeftViewer.SetZoom(1.0);
            RightViewer.SetZoom(1.0);
        }

        private void SetActiveSide(CompareSide side)
        {
            _activeSide = side;
            ActiveSideTextBlock.Text = side == CompareSide.Left ? "当前：左图" : "当前：右图";
        }

        private void SetActiveSideRating(int rating)
        {
            if (_activeSide == CompareSide.Left)
            {
                SetRating(_leftIndex, rating, isLeft: true);
            }
            else
            {
                SetRating(_rightIndex, rating, isLeft: false);
            }
        }

        private void ToggleActiveSideFavorite()
        {
            if (_activeSide == CompareSide.Left)
            {
                ToggleFavorite(_leftIndex, isLeft: true);
            }
            else
            {
                ToggleFavorite(_rightIndex, isLeft: false);
            }
        }

        private void ToggleActiveSidePicked()
        {
            if (_activeSide == CompareSide.Left)
            {
                TogglePicked(_leftIndex, isLeft: true);
            }
            else
            {
                TogglePicked(_rightIndex, isLeft: false);
            }
        }

        private void ToggleActiveSideRejected()
        {
            if (_activeSide == CompareSide.Left)
            {
                ToggleRejected(_leftIndex, isLeft: true);
            }
            else
            {
                ToggleRejected(_rightIndex, isLeft: false);
            }
        }

        private void LeftViewer_ViewChanged(object? sender, EventArgs e)
        {
            SyncView(LeftViewer, RightViewer);
        }

        private void RightViewer_ViewChanged(object? sender, EventArgs e)
        {
            SyncView(RightViewer, LeftViewer);
        }

        private void LeftPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            MoveLeft(-1);
        }

        private void LeftNextButton_Click(object sender, RoutedEventArgs e)
        {
            MoveLeft(1);
        }

        private void RightPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            MoveRight(-1);
        }

        private void RightNextButton_Click(object sender, RoutedEventArgs e)
        {
            MoveRight(1);
        }

        private void FitBothButton_Click(object sender, RoutedEventArgs e)
        {
            FitBoth();
        }

        private void ActualSizeBothButton_Click(object sender, RoutedEventArgs e)
        {
            SetBothActualSize();
        }

        private void FavoriteLeftButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSide(CompareSide.Left);
            ToggleFavorite(_leftIndex, isLeft: true);
        }

        private void FavoriteRightButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSide(CompareSide.Right);
            ToggleFavorite(_rightIndex, isLeft: false);
        }

        private void LeftRatingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingRatingControls)
            {
                return;
            }

            SetActiveSide(CompareSide.Left);
            SetRating(_leftIndex, LeftRatingComboBox.SelectedIndex, isLeft: true);
        }

        private void RightRatingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingRatingControls)
            {
                return;
            }

            SetActiveSide(CompareSide.Right);
            SetRating(_rightIndex, RightRatingComboBox.SelectedIndex, isLeft: false);
        }

        private void LeftPickButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSide(CompareSide.Left);
            TogglePicked(_leftIndex, isLeft: true);
        }

        private void RightPickButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSide(CompareSide.Right);
            TogglePicked(_rightIndex, isLeft: false);
        }

        private void LeftRejectButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSide(CompareSide.Left);
            ToggleRejected(_leftIndex, isLeft: true);
        }

        private void RightRejectButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSide(CompareSide.Right);
            ToggleRejected(_rightIndex, isLeft: false);
        }

        private void LeftImageComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelection)
            {
                return;
            }

            ReplaceLeftImage(LeftImageComboBox.SelectedIndex);
        }

        private void RightImageComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelection)
            {
                return;
            }

            ReplaceRightImage(RightImageComboBox.SelectedIndex);
        }

        private void LeftSide_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetActiveSide(CompareSide.Left);
        }

        private void RightSide_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetActiveSide(CompareSide.Right);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (TryHandleRatingKey(e.Key))
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.A)
            {
                MoveLeft(-1);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.D)
            {
                MoveLeft(1);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.J)
            {
                MoveRight(-1);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.L)
            {
                MoveRight(1);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.S)
            {
                SyncViewCheckBox.IsChecked = !IsSyncEnabled();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.F)
            {
                ToggleActiveSideFavorite();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.P)
            {
                ToggleActiveSidePicked();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.X)
            {
                ToggleActiveSideRejected();
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

            SetActiveSideRating(rating);
            return true;
        }
    }
}
