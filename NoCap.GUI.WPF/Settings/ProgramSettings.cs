using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NoCap.Library;
using WinputDotNet;

namespace NoCap.GUI.WPF.Settings {
    public class ProgramSettings {
        public IInputProvider InputProvider {
            get;
            set;
        }

        public ObservableCollection<SourceDestinationCommandBinding> Bindings {
            get;
            set;
        }

        public ObservableCollection<SourceDestinationCommand> Commands {
            get;
            set;
        }

        public ProgramSettings() :
            this(Providers.Instance) {
        }

        public ProgramSettings(Providers providers) {
            InputProvider = providers.InputProviders.FirstOrDefault();
            Bindings = new ObservableCollection<SourceDestinationCommandBinding>();
            Commands = new ObservableCollection<SourceDestinationCommand>();
        }

        /// <summary>
        /// Deep clones this instance.
        /// </summary>
        /// <returns>A cloned copy of this instance.</returns>
        public ProgramSettings Clone() {
            // I have a feeling this is a hack.

            var commandMappings = new Dictionary<SourceDestinationCommand, SourceDestinationCommand>(new ReferenceComparer());

            var getNewCommand = new Func<SourceDestinationCommand, SourceDestinationCommand>((command) => {
                if (command == null) {
                    return null;
                }

                SourceDestinationCommand newCommand;

                if (commandMappings.TryGetValue(command, out newCommand)) {
                    return newCommand;
                }

                return new SourceDestinationCommand(command.Name, command.Source, command.Destination);
            });

            var newCommands = new ObservableCollection<SourceDestinationCommand>();

            foreach (var command in Commands) {
                var newCommand = getNewCommand(command);

                commandMappings[command] = newCommand;
                newCommands.Add(newCommand);
            }

            var newBindings = new ObservableCollection<SourceDestinationCommandBinding>(
                Bindings.Select((binding) => new SourceDestinationCommandBinding(
                    binding.Input,
                    getNewCommand(binding.Command)
                ))
            );

            return new ProgramSettings {
                Bindings = newBindings,
                Commands = newCommands,
                InputProvider = InputProvider
            };
        }
    }

    public class ReferenceComparer : IEqualityComparer<object> {
        public bool Equals(object x, object y) {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj) {
            if (obj == null) {
                return 42;
            }

            return obj.GetHashCode();
        }
    }

    public interface ISettingsEditor {
        // TODO Make these dependancy properties?

        string DisplayName {
            get;
        }

        ProgramSettings ProgramSettings {
            get;
        }
    }
    
    public class SourceDestinationCommandBinding : ICommandBinding {
        public IInputSequence Input {
            get;
            set;
        }

        ICommand ICommandBinding.Command {
            get {
                return Command;
            }
        }

        public SourceDestinationCommand Command {
            get;
            set;
        }

        public SourceDestinationCommandBinding(IInputSequence input, SourceDestinationCommand command) {
            Input = input;
            Command = command;
        }
    }

    public class SourceDestinationCommand : ICommand {
        private readonly ISource source;
        private readonly IDestination destination;
        private readonly string name;

        public string Name {
            get {
                return this.name;
            }
        }

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

        public SourceDestinationCommand(string name, ISource source, IDestination destination) {
            this.name = name;
            this.source = source;
            this.destination = destination;
        }
    }
}
