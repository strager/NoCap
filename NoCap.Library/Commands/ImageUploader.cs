using System.Collections.Generic;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    public abstract class ImageUploader : HttpUploader {
        public ImageWriter ImageWriter {
            get;
            set;
        }

        protected ImageUploader(ImageWriter writer) {
            ImageWriter = writer;
        }

        public override TypedData Process(TypedData data, IMutableProgressTracker progress) {
            switch (data.DataType) {
                case TypedDataType.Image:
                    var rawImageProgress = new NotifyingProgressTracker();
                    var uploadProgress = new NotifyingProgressTracker();
                    var aggregateProgress = new AggregateProgressTracker(rawImageProgress, uploadProgress);
                    aggregateProgress.BindTo(progress);

                    using (var rawImageData = ImageWriter.Process(data, rawImageProgress)) {
                        return Upload(rawImageData, uploadProgress);
                    }

                default:
                    return null;
            }
        }

        public override IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Image };
        }

        public override IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Uri };
        }
    }
}
