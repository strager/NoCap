using System.Linq;

namespace NoCap.Library {
    public static class InfoStuff {
        public static ICommand GetDefaultCommand(this IInfoStuff infoStuff, CommandFeatures features) {
            var sharedCommands = infoStuff.Commands.Where((command) => command.GetFactory().CommandFeatures.HasFlag(features));

            var factories = infoStuff.CommandFactories.Where((factory) => factory.CommandFeatures.HasFlag(features));

            if (sharedCommands.Any()) {
                return sharedCommands.First();
            }

            if (factories.Any()) {
                return factories.First().CreateCommand(infoStuff);
            }

            return null;
        }
    }
}