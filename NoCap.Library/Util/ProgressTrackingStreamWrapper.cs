using System;
using System.IO;
using NoCap.Library.Progress;

namespace NoCap.Library.Util {
    public class ProgressTrackingStreamWrapper : Stream, IProgressTracker {
        private readonly Stream wrappedStream;
        private long expectedLength;
        private long writtenLength;

        public long ExpectedLength {
            get {
                return this.expectedLength;
            }

            set {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.expectedLength = value;

                OnProgressUpdated(new ProgressUpdatedEventArgs(Progress));
            }
        }

        public long WrittenLength {
            get {
                return this.writtenLength;
            }

            private set {
                this.writtenLength = value;

                OnProgressUpdated(new ProgressUpdatedEventArgs(Progress));
            }
        }

        public double Progress {
            get {
                return (double) WrittenLength / ExpectedLength;
            }
        }

        public event EventHandler<ProgressUpdatedEventArgs> ProgressUpdated;

        public void OnProgressUpdated(ProgressUpdatedEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }

        public ProgressTrackingStreamWrapper(Stream wrappedStream, long expectedLength) {
            ExpectedLength = expectedLength;

            this.wrappedStream = wrappedStream;

            WrittenLength = 0;
        }

        public override void Flush() {
            this.wrappedStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            this.wrappedStream.Write(buffer, offset, count);

            WrittenLength += count;
        }

        public override bool CanRead {
            get {
                return false;
            }
        }

        public override bool CanSeek {
            get {
                return false;
            }
        }

        public override bool CanWrite {
            get {
                return this.wrappedStream.CanWrite;
            }
        }

        public override long Length {
            get {
                throw new NotSupportedException();
            }
        }

        public override long Position {
            get {
                return this.wrappedStream.Position;
            }

            set {
                throw new NotSupportedException();
            }
        }
    }
}