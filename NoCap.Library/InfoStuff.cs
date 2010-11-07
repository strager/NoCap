using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library {
    public static class InfoStuff {
        public static IEnumerable<ICommand> GetTextUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Commands.Where((command) => command.GetFactory().CommandFeatures.HasFlag(CommandFeatures.TextUploader));
        }
        
        public static IEnumerable<ICommand> GetImageUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Commands.Where((command) => command.GetFactory().CommandFeatures.HasFlag(CommandFeatures.ImageUploader));
        }
        
        public static IEnumerable<ICommand> GetUrlShorteners(this IInfoStuff infoStuff) {
            return infoStuff.Commands.Where((command) => command.GetFactory().CommandFeatures.HasFlag(CommandFeatures.UrlShortener));
        }
        
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