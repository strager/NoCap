namespace NoCap.Library.Destinations {
    public abstract class ImageUploader : HttpUploader {
        public ImageWriter ImageWriter {
            get;
            set;
        }

        protected ImageUploader(ImageWriter writer) {
            ImageWriter = writer;
        }

        public override IOperation<TypedData> Put(TypedData data) {
            switch (data.Type) {
                case TypedDataType.Image:
                    var convertOp = ImageWriter.Put(data);
                    var rootOp = new EasyOperation<TypedData>((op) => {
                        convertOp.Start();

                        return null;
                    });

                    convertOp.Completed += (sender1, e1) => {
                        var uploadOp = Upload(e1.Data);
                        uploadOp.Completed += (sender2, e2) => {
                            rootOp.Done(e2.Data);
                        };

                        uploadOp.Start();
                    };

                    return rootOp;

                default:
                    return null;
            }
        }
    }
}
