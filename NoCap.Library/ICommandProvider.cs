using Bindable.Linq;

namespace NoCap.Library {
    /// <summary>
    /// Provides extensions with commands published by other extensions
    /// and commands configured by the user.
    /// </summary>
    public interface ICommandProvider {
        /// <summary>
        /// Gets the command factories published by all loaded extensions.
        /// </summary>
        /// <value>The command factories.</value>
        IBindableCollection<ICommandFactory> CommandFactories { get; }

        // TODO All commands?
        /// <summary>
        /// Gets the stand alone commands.
        /// </summary>
        /// <value>The stand alone commands.</value>
        IBindableCollection<ICommand> StandAloneCommands { get; }

        /// <summary>
        /// Gets the user-specified default command for the given features.
        /// </summary>
        /// <param name="features">The features.</param>
        /// <returns>The default command for the given features.</returns>
        ICommand GetDefaultCommand(CommandFeatures features);

        /// <summary>
        /// Determines whether the specified command is a user-specified default.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>
        /// <c>true</c> if the specified command is a default command; otherwise, <c>false</c>.
        /// </returns>
        bool IsDefaultCommand(ICommand command);
    }
}