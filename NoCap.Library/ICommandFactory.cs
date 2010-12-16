using NoCap.Library.Extensions;

namespace NoCap.Library {
    /// <summary>
    /// Represents a factory which can produce instances of commands
    /// and editors of those command instances.
    /// </summary>
    public interface ICommandFactory : INamedComponent {
        /// <summary>
        /// Creates a new command instance.
        /// </summary>
        /// <returns>A new command instance.</returns>
        ICommand CreateCommand();

        /// <summary>
        /// Populates the command with defaults from the given command provider.
        /// </summary>
        /// <param name="command">The command to populate.</param>
        /// <param name="commandProvider">The command provider.</param>
        void PopulateCommand(ICommand command, ICommandProvider commandProvider);

        /// <summary>
        /// Gets an editor which can be used to edit the given command
        /// instance, or <c>null</c> if no editor is needed or can be provided.
        /// </summary>
        /// <param name="commandProvider">The command provider.</param>
        /// <returns>
        /// A new instance of an editor for commands produced by
        /// <see cref="CreateCommand"/>, or <c>null</c> if no
        /// editor is required.
        /// </returns>
        ICommandEditor GetCommandEditor(ICommandProvider commandProvider);

        /// <summary>
        /// Gets the features a command created using <see cref="CreateCommand"/>
        /// features.
        /// </summary>
        /// <value>The command features.</value>
        CommandFeatures CommandFeatures { get; }
    }
}
