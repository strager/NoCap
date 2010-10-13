using System.Collections.Generic;

namespace NoCap.Library.Destinations {
    public abstract class ImageUploader : HttpUploader {
        // TODO Accept *raw data* instead of an image (less coupling)

        public ImageWriter ImageWriter {
            get;
            set;
        }

        protected ImageUploader(ImageWriter writer) {
            ImageWriter = writer;
        }

        public override TypedData Put(TypedData data, IMutableProgressTracker progress) {
            switch (data.DataType) {
                case TypedDataType.Image:
                    var rawImageData = ImageWriter.Put(data, progress);

                    return Upload(rawImageData, progress);

                default:
                    return null;
            }
        }

        public override IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Image };
        }

        public override System.Collections.Generic.IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Uri };
        }
    }
}
