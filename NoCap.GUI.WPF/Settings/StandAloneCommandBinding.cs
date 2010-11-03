using System;
using WinputDotNet;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF.Settings {
    [Serializable]
    public sealed class StandAloneCommandBinding : ICommandBinding {
        private readonly IInputSequence input;
        private readonly ICommand command;

        public IInputSequence Input {
            get {
                return this.input;
            }
        }

        WinputDotNet.ICommand ICommandBinding.Command {
            get {
                return new BoundCommand(Command);
            }
        }

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public StandAloneCommandBinding(IInputSequence input, ICommand command) {
            this.input = input;
            this.command = command;
        }
    }
}