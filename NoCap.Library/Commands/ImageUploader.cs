﻿using System;
using System.Collections.Generic;
using System.Threading;
using NoCap.Library.Imaging;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    [Serializable]
    public abstract class ImageUploader : HttpUploader {
        public ImageWriter ImageWriter {
            get;
            set;
        }

        protected ImageUploader(ImageWriter writer) {
            ImageWriter = writer;
        }

        public override TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (data.DataType) {
                case TypedDataType.Image:
                    var rawImageProgress = new NotifyingProgressTracker(ImageWriter.ProcessTimeEstimate);
                    var uploadProgress = new NotifyingProgressTracker(ProcessTimeEstimate);
                    var aggregateProgress = new AggregateProgressTracker(rawImageProgress, uploadProgress);
                    aggregateProgress.BindTo(progress);

                    using (var rawImageData = ImageWriter.Process(data, rawImageProgress, cancelToken)) {
                        return Upload(rawImageData, uploadProgress, cancelToken);
                    }

                default:
                    return null;
            }
        }
    }
}
