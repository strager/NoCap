using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NoCap.GUI.WPF.Commands;
using NoCap.Library;
using WinputDotNet;

namespace NoCap.GUI.WPF.Settings {
    public class ProgramSettings {
        public IInputProvider InputProvider {
            get;
            set;
        }

        public ObservableCollection<TemplateBinding> Bindings {
            get;
            set;
        }

        public ObservableCollection<HighLevelCommand> Commands {
            get;
            set;
        }

        public ObservableCollection<IProcessor> Processors {
            get;
            set;
        }

        public ProgramSettings() :
            this(Providers.Instance) {
        }

        public ProgramSettings(Providers providers) {
            InputProvider = providers.InputProviders.FirstOrDefault();
            Bindings = new ObservableCollection<TemplateBinding>();
            Commands = new ObservableCollection<HighLevelCommand>();
        }

        /// <summary>
        /// Deep clones this instance.
        /// </summary>
        /// <returns>A cloned copy of this instance.</returns>
        public ProgramSettings Clone() {
            // I have a feeling this is a hack.
            // It is a hack.
            // =[

            return this; // Fuck it.
            // XXX THIS IS WHY THE "CANCEL" BUTTON OF THE SETTINGS DIALOG SAVES XXX

            var templateMappings = new Dictionary<HighLevelCommand, HighLevelCommand>(new ReferenceComparer());

            var getCommand = new Func<HighLevelCommand, HighLevelCommand>((command) => {
                if (command == null) {
                    return null;
                }

                HighLevelCommand newCommand;

                if (templateMappings.TryGetValue(command, out newCommand)) {
                    return newCommand;
                }

                //return command.Clone();
                return null;
            });

            var newCommands = new ObservableCollection<HighLevelCommand>();

            foreach (var command in Commands) {
                var newCommand = getCommand(command);

                templateMappings[command] = newCommand;
                newCommands.Add(newCommand);
            }

            var newBindings = new ObservableCollection<TemplateBinding>(
                Bindings.Select((binding) => new TemplateBinding(
                    binding.Input,
                    getCommand(binding.HighLevelCommand)
                ))
            );

            return new ProgramSettings {
                Bindings = newBindings,
                Commands = newCommands,
                //Processors = newProcessors,
                InputProvider = InputProvider
            };
        }
    }

    public class ReferenceComparer : IEqualityComparer<object> {
        bool IEqualityComparer<object>.Equals(object x, object y) {
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
    
    public sealed class TemplateBinding : ICommandBinding {
        private readonly IInputSequence input;
        private readonly HighLevelCommand highLevelCommand;

        public IInputSequence Input {
            get {
                return this.input;
            }
        }

        WinputDotNet.ICommand ICommandBinding.Command {
            get {
                return this.HighLevelCommand;
            }
        }

        public HighLevelCommand HighLevelCommand {
            get {
                return this.highLevelCommand;
            }
        }

        public TemplateBinding(IInputSequence input, HighLevelCommand highLevelCommand) {
            this.input = input;
            this.highLevelCommand = highLevelCommand;
        }
    }
}
