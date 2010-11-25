using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NoCap.Library {
    public interface ICommandProvider {
        IEnumerable<ICommandFactory> CommandFactories { get; }

        ICommand GetDefaultCommand(CommandFeatures features);
        bool IsDefaultCommand(ICommand command);
    }

    public static class CommandProvider {
        public static ICommand GetPreferredCommand(this ICommandProvider commandProvider, CommandFeatures commandFeatures) {
            var factory = commandProvider.GetPreferredCommandFactory(commandFeatures);

            if (factory == null) {
                return null;
            }

            return factory.CreateCommand();
        }

        public static ICommandFactory GetPreferredCommandFactory(this ICommandProvider commandProvider, CommandFeatures features) {
            return GetPreferredCommandFactory(commandProvider.CommandFactories, features);
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