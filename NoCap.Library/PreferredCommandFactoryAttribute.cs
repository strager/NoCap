using System;

namespace NoCap.Library {
    /// <summary>
    /// Marks the command factory as being preferred for the specified features.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class PreferredCommandFactoryAttribute : Attribute {
        private readonly CommandFeatures commandFeatures;

        /// <summary>
        /// Marks the command factory as being preferred for the specified features.
        /// </summary>
        /// <param name="commandFeatures">
        /// The command features for which this class is preferred.
        /// </param>
        public PreferredCommandFactoryAttribute(CommandFeatures commandFeatures) {
            this.commandFeatures = commandFeatures;
        }

        /// <summary>
        /// Gets the command features for which this class is preferred.
        /// </summary>
        /// <value>The command features for which this class is preferred.</value>
        public CommandFeatures CommandFeatures {
            get {
                return this.commandFeatures;
            }
        }
    }
}
