// http://blogs.msdn.com/b/dwayneneed/archive/2007/10/05/blurry-bitmaps.aspx

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NoCap.Plugins.Helpers {
    internal class BitmapElement : UIElement {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(BitmapSource),
            typeof(BitmapElement),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnSourceChanged
            )
        );

        private readonly EventHandler sourceDownloaded;
        private readonly EventHandler<ExceptionEventArgs> sourceFailed;
        private Point pixelOffset;

        public BitmapElement() {
            this.sourceDownloaded = new EventHandler(OnSourceDownloaded);
            this.sourceFailed = new EventHandler<ExceptionEventArgs>(OnSourceFailed);

            LayoutUpdated += OnLayoutUpdated;
        }

        public BitmapSource Source {
            get {
                return (BitmapSource) GetValue(SourceProperty);
            }

            set {
                SetValue(SourceProperty, value);
            }
        }

        public event EventHandler<ExceptionEventArgs> BitmapFailed;

        // Return our measure size to be the size needed to display the bitmap pixels.
        protected override Size MeasureCore(Size availableSize) {
            var bitmapSource = Source;

            if (bitmapSource == null) {
                return default(Size);
            }

            var ps = PresentationSource.FromVisual(this);

            if (ps == null) {
                return default(Size);
            }

            var fromDevice = ps.CompositionTarget.TransformFromDevice;

            var pixelSize = new Vector(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            var measureSizeV = fromDevice.Transform(pixelSize);

            return new Size(measureSizeV.X, measureSizeV.Y);
        }

        protected override void OnRender(DrawingContext dc) {
            var bitmapSource = Source;

            if (bitmapSource == null) {
                return;
            }

            this.pixelOffset = GetPixelOffset();

            // Render the bitmap offset by the needed amount to align to pixels.
            dc.DrawImage(bitmapSource, new Rect(this.pixelOffset, DesiredSize));
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var bitmapElement = (BitmapElement) d;

            var oldValue = (BitmapSource) e.OldValue;
            var newValue = (BitmapSource) e.NewValue;

            if (oldValue != null && bitmapElement.sourceDownloaded != null && !oldValue.IsFrozen) {
                oldValue.DownloadCompleted -= bitmapElement.sourceDownloaded;
                oldValue.DownloadFailed    -= bitmapElement.sourceFailed;
            }

            if (newValue != null && !newValue.IsFrozen) {
                if (newValue.IsDownloading) {
                    newValue.DownloadCompleted += bitmapElement.sourceDownloaded;
                    newValue.DownloadFailed += bitmapElement.sourceFailed;
                } else {
                    bitmapElement.OnSourceDownloaded(newValue, new EventArgs());
                }
            }
        }

        private void OnSourceDownloaded(object sender, EventArgs e) {
            InvalidateMeasure();
            InvalidateVisual();
        }

        private void OnSourceFailed(object sender, ExceptionEventArgs e) {
            Source = null; // setting a local value seems sketchy...

            BitmapFailed(this, e);
        }

        private void OnLayoutUpdated(object sender, EventArgs e) {
            // This event just means that layout happened somewhere.  However, this is
            // what we need since layout anywhere could affect our pixel positioning.
            var curPixelOffset = GetPixelOffset();

            if (!AreClose(curPixelOffset, this.pixelOffset)) {
                InvalidateVisual();
            }
        }

        // Gets the matrix that will convert a point from "above" the
        // coordinate space of a visual into the the coordinate space
        // "below" the visual.
        private static Matrix GetVisualTransform(Visual v) {
            if (v == null) {
                return Matrix.Identity;
            }

            var m = Matrix.Identity;

            var transform = VisualTreeHelper.GetTransform(v);

            if (transform != null) {
                m = Matrix.Multiply(m, transform.Value);
            }

            var offset = VisualTreeHelper.GetOffset(v);
            m.Translate(offset.X, offset.Y);

            return m;
        }

        private static bool TryApplyVisualTransform(Point point, Visual v, bool inverse, bool throwOnError, out Point result) {
            if (v == null) {
                result = point;

                return true;
            }

            var visualTransform = GetVisualTransform(v);

            if (inverse) {
                if (!throwOnError && !visualTransform.HasInverse) {
                    result = default(Point);

                    return false;
                }

                visualTransform.Invert();
            }

            result = visualTransform.Transform(point);

            return true;
        }

        private static Point ApplyVisualTransform(Point point, Visual v, bool inverse) {
            Point result;

            TryApplyVisualTransform(point, v, inverse, true, out result);

            return result;
        }

        private Point GetPixelOffset() {
            var curPixelOffset = new Point();

            var ps = PresentationSource.FromVisual(this);

            if (ps == null) {
                return curPixelOffset;
            }

            var rootVisual = ps.RootVisual;

            // Transform (0,0) from this element up to pixels.
            curPixelOffset = TransformToAncestor(rootVisual).Transform(curPixelOffset);
            curPixelOffset = ApplyVisualTransform(curPixelOffset, rootVisual, false);
            curPixelOffset = ps.CompositionTarget.TransformToDevice.Transform(curPixelOffset);

            // Round the origin to the nearest whole pixel.
            curPixelOffset.X = Math.Round(curPixelOffset.X);
            curPixelOffset.Y = Math.Round(curPixelOffset.Y);

            // Transform the whole-pixel back to this element.
            curPixelOffset = ps.CompositionTarget.TransformFromDevice.Transform(curPixelOffset);
            curPixelOffset = ApplyVisualTransform(curPixelOffset, rootVisual, true);
            curPixelOffset = rootVisual.TransformToDescendant(this).Transform(curPixelOffset);

            return curPixelOffset;
        }

        private static bool AreClose(Point point1, Point point2) {
            return AreClose(point1.X, point2.X) && AreClose(point1.Y, point2.Y);
        }

        private static bool AreClose(double value1, double value2) {
            if (value1 == value2) {
                return true;
            }

            double delta = value1 - value2;
            return ((delta < 1.53E-06) && (delta > -1.53E-06)); // Wat.
        }
    }
}