using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;
using NoCap.Web.Multipart;

namespace NoCap.Library.Commands {
    public enum HttpRequestMethod {
        Get,
        Post,
    }

    [DataContract(Name = "HttpUploader")]
    public abstract class HttpUploaderCommand : ICommand {
        public abstract string Name { get; }

        public abstract TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken);

        public TypedData Upload(TypedData originalData, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var uploader = new HttpUploadRequest {
                Parameters = GetParameters(originalData),
                Uri = Uri,
            };

            uploader.PreprocessRequest += (sender, request) => PreprocessRequest(request);
            uploader.PreprocessRequestData += (sender, builder) => PreprocessRequestData(builder, originalData);

            try {
                var response = uploader.Execute(progress, cancelToken, RequestMethod);

                return GetResponseData(response, originalData);
            } catch (OperationCanceledException e) {
                throw CommandCanceledException.Wrap(e, this);
            }
        }

        protected abstract Uri Uri { get; }

        protected abstract IDictionary<string, string> GetParameters(TypedData data);

        protected abstract TypedData GetResponseData(HttpWebResponse response, TypedData originalData);

        protected virtual void PreprocessRequestData(MultipartBuilder helper, TypedData originalData) {
            // Do nothing
        }

        protected virtual HttpRequestMethod RequestMethod {
            get {
                return HttpRequestMethod.Post;
            }
        }

        protected virtual void PreprocessRequest(HttpWebRequest request) {
            // Do nothing
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
