using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NoCap.Library;
using Image = System.Drawing.Image;
using Point = System.Windows.Point;
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

        private readonly Crosshair crosshair = new Crosshair();

        public CropShotWindow() {
            InitializeComponent();

            Topmost = true;
            ShowInTaskbar = false;

            SourceInitialized += (sender, e) => SetFullscreen();

            Loaded    += (sender, e) => { PutOnTop(); Resize(); };
            KeyDown   += (sender, e) => KeyPressed(e.Key);
            LostFocus += (sender, e) => Close();

            MouseDown += (sender, e) => StartDragging   (e.GetPosition(this));
            MouseUp   += (sender, e) => EndDragging     (e.GetPosition(this));
            MouseMove += (sender, e) => MoveCrosshair(e.GetPosition(this));

            this.canvas.DataContext = this.crosshair;

            this.crosshair.Size = new Size(32, 32);
        }

        private void SetFullscreen() {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
        }

        private Point GetPixelPosition(Point diuPoint) {
            return new Point(
                diuPoint.X * SourceImage.Width  / Width,
                diuPoint.Y * SourceImage.Height / Height
            );
        }

        private void StartDragging(Point diuLocation) {
            this.isDragging = true;
            this.dragStart = diuLocation;
        }

        private void MoveCrosshair(Point diuLocation) {
            this.crosshair.X = diuLocation.X;
            this.crosshair.Y = diuLocation.Y;

            ContinueDragging(diuLocation);
        }

        private void ContinueDragging(Point diuLocation) {
            if (!this.isDragging) {
                return;
            }

            UpdateDragRectangle(this.dragStart, diuLocation);
        }

        private void EndDragging(Point diuLocation) {
            if (!this.isDragging) {
                return;
            }

            var region = GetPixelRectangle(diuLocation, this.dragStart);
            var selectedImage = GetSelectedImage(region);

            if (selectedImage != null) {
                Data = TypedData.FromImage(selectedImage, DataName);

                Close();
            }
        }

        private Rectangle GetPixelRectangle(Point diuA, Point diuB) {
            var pixelA = GetPixelPosition(diuA);
            var pixelB = GetPixelPosition(diuB);

            // A is not necessarily up and left of B,
            // which is why we have this min and max logic.

            return Rectangle.FromLTRB(
                (int) Math.Floor  (Math.Min(pixelB.X, pixelA.X)),
                (int) Math.Floor  (Math.Min(pixelB.Y, pixelA.Y)),
                (int) Math.Ceiling(Math.Max(pixelB.X, pixelA.X)),
                (int) Math.Ceiling(Math.Max(pixelB.Y, pixelA.Y))
            );
        }

        private void UpdateDragRectangle(Point diuA, Point diuB) {
            if (!this.isDragging) {
                this.cropRectangle.Visibility = Visibility.Hidden;
            }

            this.cropRectangle.Visibility = Visibility.Visible;

            // A is not necessarily up and left of B,
            // which is why we have this abs and min logic.

            this.cropRectangle.Width  = Math.Abs(diuA.X - diuB.X);
            this.cropRectangle.Height = Math.Abs(diuA.Y - diuB.Y);

            Canvas.SetLeft(this.cropRectangle, Math.Min(diuA.X, diuB.X));
            Canvas.SetTop (this.cropRectangle, Math.Min(diuA.Y, diuB.Y));
        }

        private Image GetSelectedImage(Rectangle pixelRegion) {
            if (pixelRegion.IsEmpty) {
                return null;
            }

            var selectedImage = new Bitmap(pixelRegion.Width, pixelRegion.Height);
            
            using (var g = Graphics.FromImage(selectedImage)) {
                g.DrawImageUnscaled(SourceImage, -pixelRegion.X, -pixelRegion.Y);
            }

            return selectedImage;
        }

        private void StopDragging() {
            this.isDragging = false;

            UpdateDragRectangle(new Point(0, 0), new Point(0, 0));
        }

        private void KeyPressed(Key keyCode) {
            if (keyCode.HasFlag(Key.Escape)) {
                if (this.isDragging) {
                    StopDragging();
                } else {
                    Close();
                }
            }
        }

        private void PutOnTop() {
            Focus();
        }
    }
}
