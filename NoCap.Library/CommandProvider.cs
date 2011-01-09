using System;
using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library {
    public static class CommandProvider {
        /// <summary>
        /// Gets a command marked as preferred for the given features.
        /// </summary>
        /// <param name="commandProvider">The command provider.</param>
        /// <param name="features">The features to test for.</param>
        /// <returns>
        /// An unpopulated instance of the preferred command type, or
        /// null if none such type exists.
        /// </returns>
        public static ICommand GetPreferredCommand(this ICommandProvider commandProvider, CommandFeatures features) {
            if (commandProvider == null) {
                throw new ArgumentNullException("commandProvider");
            }

            var factory = GetPreferredCommandFactory(commandProvider.CommandFactories, features);

            if (factory == null) {
                return null;
            }

            return factory.CreateCommand();
        }

        /// <summary>
        /// Gets the command factory marked as preferred for the given features.
        /// </summary>
        /// <param name="commandFactories">The command factories to test.</param>
        /// <param name="features">The features to test for.</param>
        /// <returns>
        /// The preferred command factory for the given features, or
        /// null if none such exists.
        /// </returns>
        public static ICommandFactory GetPreferredCommandFactory(IEnumerable<ICommandFactory> commandFactories, CommandFeatures features) {
            if (commandFactories == null) {
                throw new ArgumentNullException("commandFactories");
            }

            // If you have Resharper, try auto-refactoring this into a Linq expression.
            // I dare you.

            foreach (var factory in commandFactories) {
                var attributes = factory.GetType().GetCustomAttributes(typeof(PreferredCommandFactoryAttribute), false);

                foreach (var attribute in attributes.OfType<PreferredCommandFactoryAttribute>()) {
                    if (attribute.CommandFeatures.HasFlag(features)) {
                        return factory;
                    }
                }
            }

            return commandFactories.WithFeatures(features).FirstOrDefault();
        }
    }
}