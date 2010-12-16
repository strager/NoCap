using System;

namespace NoCap.Library {
    public static class CommandFactoryExtensions {
        /// <summary>
        /// Determines whether this factory produces commands
        /// with the specified features.
        /// </summary>
        /// <param name="factory">The command factory.</param>
        /// <param name="features">The features to test.</param>
        /// <returns>
        /// <c>true</c> if the command would have the features; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFeatures(this ICommandFactory factory, CommandFeatures features) {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }

            return factory.CommandFeatures.HasFlag(features);
        }
    }
}