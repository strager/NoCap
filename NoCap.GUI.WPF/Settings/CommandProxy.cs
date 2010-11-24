using System;
using System.Collections.Generic;
using NoCap.Library;
using NoCap.Library.Util;

namespace NoCap.GUI.WPF.Settings {
    [Serializable]
    public class CommandProxy : ICommand {
        public virtual ICommand InnerCommand {
            get;
            set;
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
            return null;
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return InnerCommand.ProcessTimeEstimate;
            }
        }

        public bool IsValid() {
            return InnerCommand.IsValidAndNotNull();
        }
    }
}