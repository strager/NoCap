using System.Collections.Specialized;
using NoCap.WebHelpers;

namespace NoCap.Destinations {
    public abstract class TextUploader : IDestination {
        public IOperation<TypedData> Put(TypedData data) {
            switch (data.Type) {
                case TypedDataType.Text:
                    return new EasyOperation<TypedData>((op) => {
                        var responseOp = HttpUploader.Upload(GetUri(), GetParameters(data));
                        responseOp.Completed += (sender, e) => {
                            op.Done(TypedData.FromUri(e.Data.ResponseUri.OriginalString, data.Name));
                        };

                        responseOp.Start();

                        return null;
                    });

                default:
                    return null;
            }
        }

        protected abstract string GetUri();
        protected abstract NameValueCollection GetParameters(TypedData data);
    }
}
