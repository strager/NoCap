using System.Windows.Input;

namespace NoCap.Library {
    /// <summary>
    /// Provides a set of nocap-related commands.
    /// </summary>
    public static class NoCapCommands {
        /// <summary>
        /// A command representing the execution of <see cref="ICommand.Process"/>.
        /// </summary>
        public static readonly RoutedUICommand Execute;

        /// <summary>
        /// A command representing the cancellation of <see cref="ICommand.Process"/>.
        /// </summary>
        public static readonly RoutedUICommand Cancel;

        static NoCapCommands() {
            Execute = new RoutedUICommand("E_xecute", "Execute", typeof(NoCapCommands));
            Cancel  = new RoutedUICommand("_Cancel",  "Cancel",  typeof(NoCapCommands));
        }
    }
}
