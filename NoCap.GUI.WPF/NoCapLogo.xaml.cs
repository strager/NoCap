using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public BitmapSource MakeIcon(int size) {
            var visual = new DrawingVisual();

            using (var context = visual.RenderOpen()) {
                context.DrawImage(this.imageSource, new Rect(0, 0, size, size));
            }

            var target = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);

            return target;
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
