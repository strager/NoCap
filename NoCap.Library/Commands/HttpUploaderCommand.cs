using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;
using NoCap.Library.Util;
using NoCap.Web.Multipart;

namespace NoCap.Library.Commands {
    [DataContract(Name = "HttpUploader")]
    public abstract class HttpUploaderCommand : ICommand {
        public abstract string Name { get; }

        public abstract TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken);

        public TypedData Upload(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            try {
                var response = HttpRequest.Execute(this.Uri, GetRequestData(data), this.RequestMethod, progress, cancelToken);

                return GetResponseData(response, data);
            } catch (OperationCanceledException e) {
                throw CommandCanceledException.Wrap(e, this);
            }
        }

        protected abstract Uri Uri { get; }

        protected abstract TypedData GetResponseData(HttpWebResponse response, TypedData originalData);

        protected virtual MultipartData GetRequestData(TypedData data) {
            return null;
        }

        protected virtual HttpRequestMethod RequestMethod {
            get {
                return HttpRequestMethod.Post;
            }
        }

        public abstract ICommandFactory GetFactory();

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.LongOperation;
            }
        }

        public virtual bool IsValid() {
            return true;
        }
    }
}
