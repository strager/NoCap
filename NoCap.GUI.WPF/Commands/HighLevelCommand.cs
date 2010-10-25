using System.Collections.Generic;
using NoCap.Library;
using NoCap.Library.Util;

namespace NoCap.GUI.WPF.Commands {
    public abstract class HighLevelCommand : WinputDotNet.ICommand, ICommand {
        public abstract string Name {
            get;
            set;
        }

        TypedData ICommand.Process(TypedData data, IMutableProgressTracker progress) {
            Execute(progress);

            return null;
        }

        public abstract ICommandFactory GetFactory();

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.None };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.None };
        }

        public abstract void Execute(IMutableProgressTracker progress);
    }
}
