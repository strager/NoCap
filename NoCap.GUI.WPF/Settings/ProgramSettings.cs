using System.Collections.Generic;
using System.Linq;
using NoCap.Library;
using WinputDotNet;

namespace NoCap.GUI.WPF.Settings {
    public class ProgramSettings {
        public IInputProvider InputProvider {
            get;
            set;
        }

        public ICollection<SourceDestinationCommandBinding> Bindings {
            get;
            set;
        }

        public ProgramSettings() :
            this(Providers.Instance) {
        }

        public ProgramSettings(Providers providers) {
            InputProvider = providers.InputProviders.FirstOrDefault();
            Bindings = new List<SourceDestinationCommandBinding>();
        }

        public ProgramSettings Clone() {
            return new ProgramSettings {
                Bindings = Bindings.ToList(),
                InputProvider = InputProvider
            };
        }
    }

    public interface ISettingsEditor {
        string DisplayName {
            get;
        }
    }
    
    public class SourceDestinationCommandBinding : ICommandBinding {
        private readonly IInputSequence input;
        private readonly SourceDestinationCommand command;

        public IInputSequence Input {
            get {
                return this.input;
            }
        }

        ICommand ICommandBinding.Command {
            get {
                return this.command;
            }
        }

        public SourceDestinationCommand Command {
            get {
                return this.command;
            }
        }

        public SourceDestinationCommandBinding(IInputSequence input, SourceDestinationCommand command) {
            this.command = command;
            this.input = input;
        }
    }

    public class SourceDestinationCommand : ICommand {
        private readonly ISource source;
        private readonly IDestination destination;

        public ISource Source {
            get {
                return this.source;
            }
        }

        public IDestination Destination {
            get {
                return this.destination;
            }
        }

        public SourceDestinationCommand(ISource source, IDestination destination) {
            this.destination = destination;
            this.source = source;
        }
    }
}
