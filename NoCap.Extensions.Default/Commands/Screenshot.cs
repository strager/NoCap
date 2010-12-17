using System.Runtime.Serialization;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Extensions.Default.Helpers;
using NoCap.Library;
using NoCap.Library.Progress;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "Screenshot")]
    public sealed class Screenshot : ICommand, IExtensibleDataObject {
        public string Name {
            get { return "Screenshot"; }
        }

        [DataMember(Name = "Source")]
        public ScreenshotSourceType SourceType {
            get;
            set;
        }

        public Screenshot() {
            SourceType = ScreenshotSourceType.EntireDesktop;
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (SourceType) {
                case ScreenshotSourceType.EntireDesktop:
                    var image = ScreenCapturer.CaptureEntireDesktop();

                    progress.Progress = 1;  // Can't really track progress on this...

                    return TypedData.FromImage(image, "screenshot");

                default:
                    return null;
            }
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

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
    
    public enum ScreenshotSourceType {
        EntireDesktop
    }
}
