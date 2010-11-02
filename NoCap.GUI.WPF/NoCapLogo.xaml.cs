using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for NoCapLogo.xaml
    /// </summary>
    public partial class NoCapLogo : INotifyPropertyChanged {
        private double progress;

        public double Progress {
            get {
                return this.progress;
            }

            set {
                if (value < 0 || value > 1) {
                    throw new ArgumentOutOfRangeException("value", "Value must be between 0 and 1");
                }

                this.progress = value;

                Notify("Progress");
                Notify("Progress2");
            }
        }

        public double Progress2 {
            get {
                return this.progress + double.Epsilon;
            }
        }

        public Icon MakeIcon(int size) {
            var visual = new DrawingVisual();

            using (var context = visual.RenderOpen()) {
                context.DrawImage(this.imageSource, new Rect(0, 0, size, size));
            }

            var target = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);

            return BitmapSourceToIcon(target);
        }

        private static Icon BitmapSourceToIcon(BitmapSource target) {
            using (var bitmap = BitmapSourceToBitmap(target)) {
                var iconHandle = bitmap.GetHicon();

                return Icon.FromHandle(iconHandle);
            }
        }

        private static Bitmap BitmapSourceToBitmap(BitmapSource target) {
            if (target.Format != PixelFormats.Pbgra32) {
                throw new NotImplementedException("Must use PABGR32 format");
            }

            Int32[] data = new Int32[target.PixelWidth * target.PixelHeight];
            int stride = Math.Max(512, target.PixelWidth);  // CopyPixels needs stride > 512

            target.CopyPixels(data, stride, 0);

            var bitmap = new Bitmap(target.PixelWidth, target.PixelHeight, PixelFormat.Format32bppPArgb);

            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, bitmap.PixelFormat
            );
            
            bitmapData.Stride = stride;
            Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        public NoCapLogo() {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
