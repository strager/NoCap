﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Library.Sources;

namespace NoCap.Plugins {
    [Export(typeof(ISource))]
    public class ScreenshotSource : ISource {
        public ScreenshotSourceType SourceType {
            get;
            set;
        }

        public ScreenshotSource() {
            SourceType = ScreenshotSourceType.EntireDesktop;
        }

        public TypedData Get(IProgressTracker progress) {
            switch (SourceType) {
                case ScreenshotSourceType.EntireDesktop:
                    var image = ScreenCapturer.CaptureEntireDesktop();

                    return TypedData.FromImage(image, "screenshot");

                default:
                    return null;
            }
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes() {
            return new[] { TypedDataType.Image };
        }
    }
    
    public enum ScreenshotSourceType {
        EntireDesktop
    }
}
