using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace NoCap.Plugins {
    public static class ScreenCapturer {
        public static Bitmap CaptureEntireDesktop() {
            // TODO Multiple monitors.
            return CaptureScreen(Screen.PrimaryScreen);
        }

        public static Bitmap CaptureScreen(Screen screen) {
            return CaptureRegion(screen.Bounds);
        }

        public static Bitmap CaptureRegion(Rectangle region) {
            var bmp = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);

            using(var graphics = Graphics.FromImage(bmp)) {
                graphics.CopyFromScreen(region.Location, new Point(0, 0), region.Size, CopyPixelOperation.SourceCopy);
            }

            return bmp;
        }
    }
}