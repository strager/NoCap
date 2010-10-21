using System;
using System.Collections.Generic;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Plugins.Factories;
using NoCap.Plugins.Helpers;

namespace NoCap.Plugins.Processors {
    public class Screenshot : IProcessor {
        public string Name {
            get { return "Screenshot"; }
        }

        public ScreenshotSourceType SourceType {
            get;
            set;
        }

        public Screenshot() {
            SourceType = ScreenshotSourceType.EntireDesktop;
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            this.CheckValidInputType(data);

            switch (SourceType) {
                case ScreenshotSourceType.EntireDesktop:
                    var image = ScreenCapturer.CaptureEntireDesktop();

                    progress.Progress = 1;  // Can't really track progress on this...

                    return TypedData.FromImage(image, "screenshot");

                default:
                    return null;
            }
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.None };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Image };
        }

        public IProcessorFactory GetFactory() {
            return new ScreenshotFactory();
        }
    }
    
    public enum ScreenshotSourceType {
        EntireDesktop
    }
}
