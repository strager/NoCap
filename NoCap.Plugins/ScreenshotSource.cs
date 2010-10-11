using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NoCap.Library.Sources;

namespace NoCap.Plugins {
    [Export(typeof(ISource))]
    public class ScreenshotSource : ISource {
        public ScreenshotSourceType Type {
            get;
            set;
        }

        public ScreenshotSource() {
            Type = ScreenshotSourceType.EntireDesktop;
        }

        public IOperation<TypedData> Get() {
            switch (Type) {
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
