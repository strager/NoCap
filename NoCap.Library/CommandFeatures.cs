using System;
using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library {
    [Flags]
    public enum CommandFeatures {
        ImageUploader = (1 << 0),
        FileUploader = (1 << 1),
        UrlShortener = (1 << 2),
        TextUploader = (1 << 3),

        StandAlone = (1 << 8),
    };

    public static class EnumerableCommandFeaturesExtensions {
        public static IEnumerable<ICommand> WithFeatures(this IEnumerable<ICommand> commands, CommandFeatures features) {
            return commands.Where((command) => command.HasFeatures(features));
        }
        
        public static IEnumerable<ICommandFactory> WithFeatures(this IEnumerable<ICommandFactory> commands, CommandFeatures features) {
            return commands.Where((command) => command.HasFeatures(features));
        }
    }
}