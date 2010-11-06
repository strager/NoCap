using System;
using System.Runtime.Serialization;
using NoCap.Library.Util;

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
            this.command = info.GetValue<ICommand>("Command");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue("Command", command);
        }
    }
}
