using System;

namespace NoCap.Library {
    public static class CommandExtensions {
        /// <summary>
        /// Determines whether this instance has the specified features.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="features">The features to test.</param>
        /// <returns>
        /// <c>true</c> if the specified command has the features; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFeatures(this ICommand command, CommandFeatures features) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            var factory = command.GetFactory();

            if (factory == null) {
                return false;
            }

            return factory.HasFeatures(features);
        }

        /// <summary>
        /// Determines whether the command is valid and is not null.
        /// </summary>
        /// <remarks>
        /// Validity is checked using the <see cref="ICommand.IsValid"/> method.
        /// </remarks>
        /// <param name="command">The command.</param>
        /// <returns>
        /// <c>true</c> if the command is valid and is not null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidAndNotNull(this ICommand command) {
            return command != null && command.IsValid();
        }
    }
}