using System;
using System.Runtime.Serialization;

namespace NoCap.Library {
    [Serializable]
    public class CommandCanceledException : Exception {
        private readonly ICommand command;

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public CommandCanceledException() :
            this((ICommand) null) {
        }

        public CommandCanceledException(ICommand command) {
            this.command = command;
        }

        public CommandCanceledException(string message) :
            this(null, message) {
        }

        public CommandCanceledException(ICommand command, string message) :
            base(message) {
            this.command = command;
        }

        public CommandCanceledException(string message, Exception inner) :
            this(null, message, inner) {
        }

        public CommandCanceledException(ICommand command, string message, Exception inner) :
            base(message, inner) {
            this.command = command;
        }

        protected CommandCanceledException(SerializationInfo info, StreamingContext context) :
            base(info, context) {
        }
    }
}
