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

        public TypedData Upload(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var uploader = new HttpUploadRequest {
                Parameters = GetParameters(data),
                Uri = Uri,
                PostData = GetPostData(data),
            };

            uploader.PreprocessRequest += (sender, request) => PreprocessRequest(request);

            try {
                var response = uploader.Execute(progress, cancelToken, RequestMethod);

                return GetResponseData(response, data);
            } catch (OperationCanceledException e) {
                throw CommandCanceledException.Wrap(e, this);
            }
        }

        protected abstract Uri Uri { get; }

        protected virtual IDictionary<string, string> GetParameters(TypedData data) {
            return null;
        }

        protected abstract TypedData GetResponseData(HttpWebResponse response, TypedData originalData);

        protected virtual MultipartData GetPostData(TypedData data) {
            return null;
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
