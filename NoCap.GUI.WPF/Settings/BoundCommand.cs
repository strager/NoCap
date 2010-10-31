﻿using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    public sealed class BoundCommand : WinputDotNet.ICommand {
        private readonly ICommand command;

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public BoundCommand(ICommand command) {
            this.command = command;
        }
    }
}