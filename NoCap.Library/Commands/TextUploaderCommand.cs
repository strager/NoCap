using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;

namespace NoCap.Library.Commands {
    [DataContract(Name = "TextUploader")]
    public abstract class TextUploaderCommand : HttpUploaderCommand {
        public override TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (data.DataType) {
                case TypedDataType.Text:
                    progress.Status = "Uploading text";

                    return Upload(data, progress, cancelToken);

                default:
                    return null;
            }
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            return TypedData.FromUri(response.ResponseUri, originalData.Name);
        }
    }
}
