using System.Collections.Generic;
using NoCap.Library;
using NoCap.Library.Util;

namespace NoCap.GUI.WPF.Settings {
    public class CommandProxy : ICommand {
        public ICommand InnerCommand {
            get;
            set;
        }

        public CommandProxy() {
        }

        public CommandProxy(ICommand innerCommand) {
            InnerCommand = innerCommand;
        }

        public string Name {
            get {
                return InnerCommand.Name;
            }
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            return InnerCommand.Process(data, progress);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return InnerCommand.GetInputDataTypes();
        }

        public ICommandFactory GetFactory() {
            return new CommandFactoryProxy(this);
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return InnerCommand.ProcessTimeEstimate;
            }
        }
    }
}