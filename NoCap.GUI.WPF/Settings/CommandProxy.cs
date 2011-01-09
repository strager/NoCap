using System;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library;
using NoCap.Library.Progress;

namespace NoCap.GUI.WPF.Settings {
    [DataContract(Name = "CommandProxy")]
    public class CommandProxy : ICommand, IExtensibleDataObject {
        public virtual ICommand InnerCommand {
            get;
            set;
        }

        public string Name {
            get {
                return InnerCommand.Name;
            }
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            return InnerCommand.Process(data, progress, cancelToken);
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

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}