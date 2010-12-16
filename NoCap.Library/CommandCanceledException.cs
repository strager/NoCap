using System;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Util;

namespace NoCap.Library {
    /// <summary>
    /// The exception that is thrown in a thread upon cancellation
    /// of <see cref="ICommand.Process"/>.
    /// </summary>
    [Serializable]
    public class CommandCanceledException : OperationCanceledException {
        private readonly ICommand command;

        /// <summary>
        /// Gets the command which was running when the cancellation occured.
        /// </summary>
        /// <value>The command.</value>
        public ICommand Command {
            get {
                return this.command;
            }
        }

        /// <summary>
        /// Wraps the specified exception as a <see cref="CommandCanceledException"/>.
        /// </summary>
        /// <remarks>
        /// If the specified <paramref name="e"/> is an instance of
        /// <see cref="CommandCanceledException"/>, that exception's command takes
        /// preference over the <paramref name="commandSuggestion"/>.
        /// </remarks>
        /// <param name="e">The exception to wrap.</param>
        /// <param name="commandSuggestion">
        /// The command which was running when the operation was cancelled.
        /// </param>
        /// <returns>The wrapped exception.</returns>
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
