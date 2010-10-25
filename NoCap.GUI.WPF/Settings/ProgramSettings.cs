using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NoCap.GUI.WPF.Commands;
using WinputDotNet;
using ICommand = NoCap.Library.ICommand;

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

        public ObservableCollection<ICommand> Commands {
            get;
            set;
        }

        public ProgramSettings() :
            this(Providers.Instance) {
        }

        public ProgramSettings(Providers providers) {
            InputProvider = providers.InputProviders.FirstOrDefault();
            Bindings = new ObservableCollection<TemplateBinding>();
            Commands = new ObservableCollection<ICommand>();
        }

        /// <summary>
        /// Deep clones this instance.
        /// </summary>
        /// <returns>A cloned copy of this instance.</returns>
        public ProgramSettings Clone() {
            // I have a feeling this is a hack.
            // It is a hack.
            // =[
            
            // XXX THIS IS WHY THE "CANCEL" BUTTON OF THE SETTINGS DIALOG SAVES XXX
            return this; // Fuck it.
            // XXX THIS IS ALSO WHY WE CAN'T HAVE NICE THINGS XXX
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
