using NoCap.Library.Sources;
using NoCap.Sources;

namespace NoCap.Plugins {
    public class ScreenshotSource : ISource {
        private readonly ScreenshotSourceType type;

        public ScreenshotSource(ScreenshotSourceType type) {
            this.type = type;
        }

        public IOperation<TypedData> Get() {
            switch (this.type) {
                case ScreenshotSourceType.EntireDesktop:
                    return new EasyOperation<TypedData>((op) => {
                        var image = ScreenCapturer.CaptureEntireDesktop();

                        return TypedData.FromImage(image, "screenshot");
                    });

                default:
                    return null;
            }
        }
    }
    
    public enum ScreenshotSourceType {
        EntireDesktop
    }
}
