using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PhotoToneApp.Controls
{
    public partial class ZoomableImageViewer : UserControl
    {
        private const double MinZoom = 0.05;
        private const double MaxZoom = 20.0;

        private bool _isDragging;
        private bool _isFitToWindow = true;
        private bool _fitPending;
        private Point _lastDragPoint;
        private double _imageWidth;
        private double _imageHeight;

        public event EventHandler? ViewChanged;

        public double CurrentZoom => ImageScaleTransform.ScaleX;

        public double OffsetX => ImageTranslateTransform.X;

        public double OffsetY => ImageTranslateTransform.Y;

        public bool HasImage => ViewerImage.Source is not null;

        public ZoomableImageViewer()
        {
            InitializeComponent();

            Loaded += ZoomableImageViewer_Loaded;
            SizeChanged += ZoomableImageViewer_SizeChanged;
        }

        public void SetImage(ImageSource? source)
        {
            ViewerImage.Source = source;

            if (source is null)
            {
                _imageWidth = 0;
                _imageHeight = 0;
                ViewerImage.Width = 0;
                ViewerImage.Height = 0;
                ApplyView(1, 0, 0);
                _isFitToWindow = true;
                return;
            }

            (_imageWidth, _imageHeight) = GetDisplayImageSize(source);
            ViewerImage.Width = _imageWidth;
            ViewerImage.Height = _imageHeight;
            _isFitToWindow = true;

            RequestFitToWindow();
        }

        public void FitToWindow()
        {
            if (!TryGetImageSize(out double imageWidth, out double imageHeight)
                || RootGrid.ActualWidth <= 0
                || RootGrid.ActualHeight <= 0)
            {
                ApplyView(1, 0, 0);
                _isFitToWindow = true;
                return;
            }

            double scaleX = RootGrid.ActualWidth / imageWidth;
            double scaleY = RootGrid.ActualHeight / imageHeight;
            double zoom = Math.Clamp(Math.Min(scaleX, scaleY), MinZoom, MaxZoom);
            double offsetX = (RootGrid.ActualWidth - imageWidth * zoom) / 2;
            double offsetY = (RootGrid.ActualHeight - imageHeight * zoom) / 2;

            ApplyView(zoom, offsetX, offsetY);
            _isFitToWindow = true;
            RaiseViewChanged();
        }

        public void ResetView()
        {
            FitToWindow();
        }

        public void SetZoom(double zoom)
        {
            Point center = new(RootGrid.ActualWidth / 2, RootGrid.ActualHeight / 2);
            SetZoomAtPoint(zoom, center);
        }

        public void ApplyView(double zoom, double offsetX, double offsetY, bool raiseEvent = false)
        {
            double clampedZoom = Math.Clamp(zoom, MinZoom, MaxZoom);

            ImageScaleTransform.ScaleX = clampedZoom;
            ImageScaleTransform.ScaleY = clampedZoom;
            ImageTranslateTransform.X = offsetX;
            ImageTranslateTransform.Y = offsetY;
            _isFitToWindow = false;

            if (raiseEvent)
            {
                RaiseViewChanged();
            }
        }

        private void ZoomableImageViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isFitToWindow)
            {
                RequestFitToWindow();
            }
        }

        private void ZoomableImageViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isFitToWindow)
            {
                RequestFitToWindow();
            }
        }

        private void RequestFitToWindow()
        {
            if (_fitPending)
            {
                return;
            }

            _fitPending = true;
            Dispatcher.BeginInvoke(() =>
            {
                _fitPending = false;
                FitToWindow();
            }, DispatcherPriority.ContextIdle);
        }

        private void RootGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ViewerImage.Source is null)
            {
                return;
            }

            double zoomFactor = e.Delta > 0 ? 1.15 : 1 / 1.15;
            SetZoomAtPoint(CurrentZoom * zoomFactor, e.GetPosition(RootGrid));
            e.Handled = true;
        }

        private void RootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewerImage.Source is null)
            {
                return;
            }

            Focus();

            if (e.ClickCount == 2)
            {
                if (_isFitToWindow)
                {
                    SetZoom(1.0);
                }
                else
                {
                    FitToWindow();
                }

                e.Handled = true;
                return;
            }

            _isDragging = true;
            _lastDragPoint = e.GetPosition(RootGrid);
            RootGrid.CaptureMouse();
            Cursor = Cursors.SizeAll;
            e.Handled = true;
        }

        private void RootGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            Point currentPoint = e.GetPosition(RootGrid);
            Vector delta = currentPoint - _lastDragPoint;

            ImageTranslateTransform.X += delta.X;
            ImageTranslateTransform.Y += delta.Y;
            _lastDragPoint = currentPoint;
            _isFitToWindow = false;

            RaiseViewChanged();
        }

        private void RootGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;
            RootGrid.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            e.Handled = true;
        }

        private void SetZoomAtPoint(double zoom, Point centerPoint)
        {
            if (!TryGetImageSize(out _, out _))
            {
                return;
            }

            double oldZoom = CurrentZoom <= 0 ? 1 : CurrentZoom;
            double newZoom = Math.Clamp(zoom, MinZoom, MaxZoom);

            Point imagePoint = new(
                (centerPoint.X - ImageTranslateTransform.X) / oldZoom,
                (centerPoint.Y - ImageTranslateTransform.Y) / oldZoom);

            double offsetX = centerPoint.X - imagePoint.X * newZoom;
            double offsetY = centerPoint.Y - imagePoint.Y * newZoom;

            ApplyView(newZoom, offsetX, offsetY);
            _isFitToWindow = false;
            RaiseViewChanged();
        }

        private bool TryGetImageSize(out double imageWidth, out double imageHeight)
        {
            imageWidth = _imageWidth;
            imageHeight = _imageHeight;

            return imageWidth > 0 && imageHeight > 0;
        }

        private static (double Width, double Height) GetDisplayImageSize(ImageSource source)
        {
            if (source is BitmapSource bitmapSource
                && bitmapSource.PixelWidth > 0
                && bitmapSource.PixelHeight > 0)
            {
                return (bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            }

            return (Math.Max(source.Width, 1), Math.Max(source.Height, 1));
        }

        private void RaiseViewChanged()
        {
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
