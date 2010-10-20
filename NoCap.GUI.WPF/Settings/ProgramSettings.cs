using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WinputDotNet;
using ICommand = NoCap.GUI.WPF.Commands.ICommand;

namespace NoCap.GUI.WPF.Settings {
    public class ProgramSettings {
        public IInputProvider InputProvider {
            get;
            set;
        }

        public ICollection<TemplateBinding> Bindings {
            get;
            set;
        }

        public ICollection<ICommand> Commands {
            get;
            set;
        }

        public ProgramSettings() :
            this(Providers.Instance) {
        }

        public ProgramSettings(Providers providers) {
            InputProvider = providers.InputProviders.FirstOrDefault();
            Bindings = new List<TemplateBinding>();
            Commands = new List<ICommand>();
        }

        /// <summary>
        /// Deep clones this instance.
        /// </summary>
        /// <returns>A cloned copy of this instance.</returns>
        public ProgramSettings Clone() {
            // I have a feeling this is a hack.

            var templateMappings = new Dictionary<ICommand, ICommand>(new ReferenceComparer());

            var getTemplate = new Func<ICommand, ICommand>((template) => {
                if (template == null) {
                    return null;
                }

                ICommand newCommand;

                if (templateMappings.TryGetValue(template, out newCommand)) {
                    return newCommand;
                }

                return template.Clone();
            });

            var newTemplates = new ObservableCollection<ICommand>();

            foreach (var command in Commands) {
                var newCommand = getTemplate(command);

                templateMappings[command] = newCommand;
                newTemplates.Add(newCommand);
            }

            var newBindings = new ObservableCollection<TemplateBinding>(
                Bindings.Select((binding) => new TemplateBinding(
                    binding.Input,
                    getTemplate(binding.Command)
                ))
            );

            return new ProgramSettings {
                Bindings = newBindings,
                Commands = newTemplates,
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
        private readonly ICommand command;

        public IInputSequence Input {
            get {
                return this.input;
            }
        }

        WinputDotNet.ICommand ICommandBinding.Command {
            get {
                return Command;
            }
        }

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public TemplateBinding(IInputSequence input, ICommand command) {
            this.input = input;
            this.command = command;
        }
    }
}
