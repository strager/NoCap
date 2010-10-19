using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NoCap.Library;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;
using Size = System.Windows.Size;

namespace NoCap.Plugins.Helpers {
    /// <summary>
    /// Interaction logic for CropShotWindow.xaml
    /// </summary>
    public partial class CropShotWindow {
        private Image sourceImage;

        public Image SourceImage {
            get {
                return this.sourceImage;
            }

            set {
                this.sourceImage = value;

                this.imageViewer.Source = this.sourceImage.ToBitmapSource();

                Resize();
            }
        }

        private void Resize() {
            var size = this.imageViewer.DesiredSize;

            MinWidth = size.Width;
            MinHeight = size.Height;
        }

        public string DataName {
            get;
            set;
        }

        public TypedData Data {
            get;
            private set;
        }

        private Point dragStart;

        private bool isDragging;

        public CropShotWindow() {
            InitializeComponent();

            Topmost = true;
            ShowInTaskbar = false;

            SourceInitialized += (sender, e) => SetFullscreen();

            Loaded    += (sender, e) => { PutOnTop(); Resize(); };
            KeyDown   += (sender, e) => KeyPressed(e.Key);
            LostFocus += (sender, e) => Close();

            Closed += (sender, e) => {
                // TODO
            };

            MouseDown += (sender, e) => StartDragging   (GetPixelPosition(e.GetPosition(this)));
            MouseUp   += (sender, e) => EndDragging     (GetPixelPosition(e.GetPosition(this)));
            MouseMove += (sender, e) => ContinueDragging(GetPixelPosition(e.GetPosition(this)));
        }

        private void SetFullscreen() {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
        }

        private Point GetPixelPosition(System.Windows.Point diuPoint) {
            // FIXME Am I doin' it rite?
            return new Point(
                (int) Math.Round(diuPoint.X * SourceImage.Width  / Width ),
                (int) Math.Round(diuPoint.Y * SourceImage.Height / Height)
            );
        }

        private System.Windows.Point GetDiuPosition(Point pixelSize) {
            // FIXME Am I doin' it rite?
            return new System.Windows.Point(
                Math.Round((double) pixelSize.X / SourceImage.Width  * Width ),
                Math.Round((double) pixelSize.Y / SourceImage.Height * Height)
            );
        }

        private void StartDragging(Point pixelLocation) {
            this.isDragging = true;
            this.dragStart = pixelLocation;
        }

        private void ContinueDragging(Point pixelLocation) {
            if (!this.isDragging) {
                return;
            }

            UpdateDragRectangle(this.dragStart, pixelLocation);
        }

        private void EndDragging(Point pixelLocation) {
            if (!this.isDragging) {
                return;
            }

            var region = GetRectangle(pixelLocation, this.dragStart);

            var selectedImage = GetSelectedImage(region);

            if (selectedImage != null) {
                Data = TypedData.FromImage(selectedImage, DataName);

                Close();
            }
        }

        private static Rectangle GetRectangle(Point a, Point b) {
            return Rectangle.FromLTRB(
                Math.Min(b.X, a.X),
                Math.Min(b.Y, a.Y),
                Math.Max(b.X, a.X),
                Math.Max(b.Y, a.Y)
            );
        }

        private void UpdateDragRectangle(Point a, Point b) {
            if (!this.isDragging) {
                this.cropRectangle.Visibility = Visibility.Hidden;
            }

            this.cropRectangle.Visibility = Visibility.Visible;

            var region = GetRectangle(a, b);

            var diuSizePoint = GetDiuPosition(new Point(region.Size));
            var diuSize = new Size(diuSizePoint.X, diuSizePoint.Y);

            var diuTopLeft = GetDiuPosition(region.Location);

            this.cropRectangle.Width  = diuSize.Width;
            this.cropRectangle.Height = diuSize.Height;

            Canvas.SetLeft(this.cropRectangle, diuTopLeft.X);
            Canvas.SetTop (this.cropRectangle, diuTopLeft.Y);
        }

        private Image GetSelectedImage(Rectangle region) {
            if (region.IsEmpty) {
                return null;
            }

            var selectedImage = new Bitmap(region.Width, region.Height);
            
            using (var g = Graphics.FromImage(selectedImage)) {
                g.DrawImageUnscaled(SourceImage, -region.X, -region.Y);
            }

            return selectedImage;
        }

        private void StopDragging() {
            this.isDragging = false;
        }

        private void KeyPressed(Key keyCode) {
            if (keyCode.HasFlag(Key.Escape)) {
                if (this.isDragging) {
                    StopDragging();
                }

                Close();
            }
        }

        private void PutOnTop() {
            Focus();
        }
    }
}
