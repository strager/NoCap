﻿using System;
using System.Runtime.Serialization;
using WinputDotNet;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.Extensions.Default.Plugins {
    [DataContract(Name = "Binding")]
    public sealed class CommandBinding : ICommandBinding {
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

        public CommandBinding(IInputSequence input, ICommand command) {
            this.input = input;
            this.command = command;
        }
    }
}