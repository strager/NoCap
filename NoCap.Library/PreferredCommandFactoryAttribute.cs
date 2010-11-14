using System;

namespace NoCap.Library {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PreferredCommandFactoryAttribute : Attribute {
        private readonly CommandFeatures commandFeatures;

        public PreferredCommandFactoryAttribute(CommandFeatures commandFeatures) {
            this.commandFeatures = commandFeatures;
        }

        public CommandFeatures CommandFeatures {
            get {
                return this.commandFeatures;
            }
        }
    }
}
