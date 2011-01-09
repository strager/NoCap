using System;
using System.Runtime.Serialization;

namespace NoCap.Library {
    [Serializable]
    public class CommandInvalidException : Exception {
        private const string DefaultMessage = "'{0}' is not valid; check configuration";

        private readonly ICommand command;

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public CommandInvalidException(ICommand command) :
            base(string.Format(DefaultMessage, command.Name)) {
            this.command = command;
        }

        public CommandInvalidException(ICommand command, string message) :
            base(message) {
            this.command = command;
        }

        public CommandInvalidException(ICommand command, Exception innerException) :
            base(string.Format(DefaultMessage, command.Name), innerException) {
            this.command = command;
        }

        public CommandInvalidException(ICommand command, string message, Exception innerException) :
            base(message, innerException) {
            this.command = command;
        }

        protected CommandInvalidException(SerializationInfo info, StreamingContext context) :
            base(info, context) {
        }
    }
}