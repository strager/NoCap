using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library {
    public interface IInfoStuff {
        IEnumerable<ICommandFactory> CommandFactories { get; }

        ICommand GetDefaultCommand(CommandFeatures features);
        bool IsDefaultCommand(ICommand command);
    }

    public static class InfoStuff {
        public static ICommand GetPreferredCommand(this IInfoStuff infoStuff, CommandFeatures commandFeatures) {
            var factory = infoStuff.GetPreferredCommandFactory(commandFeatures);

            if (factory == null) {
                return null;
            }

            return factory.CreateCommand();
        }

        public static ICommandFactory GetPreferredCommandFactory(this IInfoStuff infoStuff, CommandFeatures features) {
            return GetPreferredCommandFactory(infoStuff.CommandFactories, features);
        }

        public static ICommandFactory GetPreferredCommandFactory(IEnumerable<ICommandFactory> commandFactories, CommandFeatures features) {
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