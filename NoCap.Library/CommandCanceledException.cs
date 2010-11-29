using System;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Util;

namespace NoCap.Library {
    [Serializable]
    public class CommandCanceledException : OperationCanceledException {
        private readonly ICommand command;

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public static CommandCanceledException Wrap(OperationCanceledException e, ICommand commandSuggestion) {
            var commandCanceledException = e as CommandCanceledException;

            if (commandCanceledException != null) {
                return new CommandCanceledException(
                    commandCanceledException.Command,
                    commandCanceledException.Message,
                    commandCanceledException,
                    commandCanceledException.CancellationToken
                );
            }

            return new CommandCanceledException(
                commandSuggestion,
                e.Message,
                e,
                e.CancellationToken
            );
        }

        public CommandCanceledException() {
        }

        public CommandCanceledException(CancellationToken token) :
            base(token) {
        }

        public CommandCanceledException(ICommand command) {
            this.command = command;
        }

        public CommandCanceledException(ICommand command, CancellationToken token) :
            base(token) {
            this.command = command;
        }

        public CommandCanceledException(string message) :
            base(message) {
        }

        public CommandCanceledException(string message, CancellationToken token) :
            base(message, token) {
        }

        public CommandCanceledException(ICommand command, string message) :
            base(message) {
            this.command = command;
        }

        public CommandCanceledException(ICommand command, string message, CancellationToken token) :
            base(message, token) {
            this.command = command;
        }

        public CommandCanceledException(string message, Exception inner) :
            base(message, inner) {
        }

        public CommandCanceledException(string message, Exception inner, CancellationToken token) :
            base(message, inner, token) {
        }

        public CommandCanceledException(ICommand command, string message, Exception inner) :
            base(message, inner) {
            this.command = command;
        }

        public CommandCanceledException(ICommand command, string message, Exception inner, CancellationToken token) :
            base(message, inner, token) {
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
