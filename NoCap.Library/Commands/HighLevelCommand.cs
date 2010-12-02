﻿using System;
using System.Collections.Generic;
using System.Threading;
using NoCap.Library.Progress;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    [Serializable]
    public abstract class HighLevelCommand : ICommand {
        public abstract string Name {
            get;
            set;
        }

        TypedData ICommand.Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            Execute(progress, cancelToken);

            return null;
        }

        public abstract ICommandFactory GetFactory();

        public abstract ITimeEstimate ProcessTimeEstimate {
            get;
        }

        public abstract bool IsValid();

        public abstract void Execute(IMutableProgressTracker progress, CancellationToken cancelToken);
    }
}
