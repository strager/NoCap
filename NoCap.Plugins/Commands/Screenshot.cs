using System;
using System.Collections.Generic;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Plugins.Factories;
using NoCap.Plugins.Helpers;

namespace NoCap.Plugins.Commands {
    [Serializable]
    public class Screenshot : ICommand {
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

        public ICommandFactory GetFactory() {
            return new ScreenshotFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.Instantanious;
            }
        }
    }
    
    public enum ScreenshotSourceType {
        EntireDesktop
    }
}
