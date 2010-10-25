﻿using System.Collections.Generic;
using NoCap.Library;
using NoCap.Library.Util;

namespace NoCap.GUI.WPF.Commands {
    public abstract class HighLevelCommand : WinputDotNet.ICommand, IProcessor {
        public abstract string Name {
            get;
            set;
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            Execute(progress);

            return null;
        }

        public abstract IProcessorFactory GetFactory();

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.None };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.None };
        }

        protected abstract void Execute(IMutableProgressTracker progress);
    }
}