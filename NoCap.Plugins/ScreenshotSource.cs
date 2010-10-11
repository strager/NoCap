using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NoCap.Library.Sources;

namespace NoCap.Plugins {
    [Export]
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

        public IEnumerable<TypedDataType> GetOutputDataTypes() {
            return new[] {
                TypedDataType.Image
            };
        }
    }
    
    public enum ScreenshotSourceType {
        EntireDesktop
    }
}
