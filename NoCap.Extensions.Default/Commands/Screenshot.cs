using System;
using System.Collections.Generic;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Extensions.Default.Helpers;
using NoCap.Library;
using NoCap.Library.Util;

namespace NoCap.Extensions.Default.Commands {
    [Serializable]
    public sealed class Screenshot : ICommand {
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

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
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

        public ICommandFactory GetFactory() {
            return new ScreenshotFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.Instantaneous;
            }
        }

        public bool IsValid() {
            return true;
        }
    }
    
    public enum ScreenshotSourceType {
        EntireDesktop
    }
}
