using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Imaging;
using NoCap.Library.Progress;

namespace NoCap.Library.Commands {
    [DataContract(Name = "ImageUploader")]
    public abstract class ImageUploader : HttpUploader {
        [DataMember(Name = "ImageWriter")]
        public ImageWriter ImageWriter {
            get;
            protected set;
        }

        protected ImageUploader(ImageWriter writer) {
            ImageWriter = writer;
        }

        public override TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (data.DataType) {
                case TypedDataType.Image:
                    var rawImageProgress = new MutableProgressTracker();
                    var uploadProgress = new MutableProgressTracker();

                    var aggregateProgress = new AggregateProgressTracker(new [] {
                        new AggregateProgressTrackerInformation(rawImageProgress, ImageWriter.ProcessTimeEstimate.ProgressWeight),
                        new AggregateProgressTrackerInformation(uploadProgress, ProcessTimeEstimate.ProgressWeight), 
                    });

                    aggregateProgress.BindTo(progress);

                    using (var rawImageData = ImageWriter.Process(data, rawImageProgress, cancelToken)) {
                        return Upload(rawImageData, uploadProgress, cancelToken);
                    }

                default:
                    return null;
            }
        }
    }
}
