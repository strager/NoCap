using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;

namespace NoCap.Library.Imaging {
    [DataContract(Name = "BitmapCodec")]
    public abstract class BitmapCodec : ICommand {
        [IgnoreDataMember]
        public virtual string Name {
            get {
                return string.Format("{0} codec", Description);
            }
        }

        [IgnoreDataMember]
        public abstract string Extension { get; }

        [IgnoreDataMember]
        public abstract string Description { get; }

        [IgnoreDataMember]
        public abstract bool CanEncode { get; }

        [IgnoreDataMember]
        public abstract bool CanDecode { get; }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (data.DataType) {
                case TypedDataType.Image:
                    return TypedData.FromStream(Encode((Bitmap) data.Data, progress, cancelToken), data.Name);

                case TypedDataType.Stream:
                    return TypedData.FromImage(Decode((Stream) data.Data, progress, cancelToken), data.Name);

                default:
                    throw new NotSupportedException();
            }
        }

        protected virtual Stream Encode(Bitmap image, IMutableProgressTracker progress, CancellationToken cancelToken) {
            throw new NotSupportedException();
        }

        protected virtual Bitmap Decode(Stream stream, IMutableProgressTracker progress, CancellationToken cancelToken) {
            throw new NotSupportedException();
        }

        ICommandFactory ICommand.GetFactory() {
            return GetFactory();
        }

        public abstract BitmapCodecFactory GetFactory();

        [IgnoreDataMember]
        public virtual ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.ShortOperation;
            }
        }

        public virtual bool IsValid() {
            return true;
        }
    }
}