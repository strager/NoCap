using System;
using System.Runtime.Serialization;

namespace NoCap.Library {
    [Serializable]
    public class CommandCancelledException : Exception {
        private readonly ICommand command;

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public CommandCancelledException() :
            this((ICommand) null) {
        }

        public CommandCancelledException(ICommand command) {
            this.command = command;
        }

        public CommandCancelledException(string message) :
            this(null, message) {
        }

        public CommandCancelledException(ICommand command, string message) :
            base(message) {
            this.command = command;
        }

        public CommandCancelledException(string message, Exception inner) :
            this(null, message, inner) {
        }

        public CommandCancelledException(ICommand command, string message, Exception inner) :
            base(message, inner) {
            this.command = command;
        }

        protected CommandCancelledException(SerializationInfo info, StreamingContext context) :
            base(info, context) {
        }
    }
}
