using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoCap.Destinations;

namespace NoCap.Sources {
    public class ScreenshotSource : ISource {
        private readonly ScreenshotSourceType type;

        public ScreenshotSource(ScreenshotSourceType type) {
            this.type = type;
        }

        public bool Get(ISourceResultThing sourceResultThing) {
            switch (this.type) {
                case ScreenshotSourceType.EntireDesktop:
                    sourceResultThing.Start();

                    var image = ScreenCapturer.CaptureEntireDesktop();

                    sourceResultThing.Done(DestinationType.Image, image, "screenshot");

                    return true;

                default:
                    return false;
            }
        }
    }

    public enum ScreenshotSourceType {
        EntireDesktop
    }
}
