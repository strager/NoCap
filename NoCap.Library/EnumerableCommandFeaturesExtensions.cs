using System.Collections.Generic;
using System.Linq;
using Bindable.Linq;

namespace NoCap.Library {
    public static class EnumerableCommandFeaturesExtensions {
        public static IEnumerable<ICommand> WithFeatures(this IEnumerable<ICommand> commands, CommandFeatures features) {
            return commands.Where((command) => CommandExtensions.HasFeatures(command, features));
        }
        
        public static IEnumerable<ICommandFactory> WithFeatures(this IEnumerable<ICommandFactory> commands, CommandFeatures features) {
            return commands.Where((command) => command.HasFeatures(features));
        }

        public static IBindableCollection<ICommand> WithFeatures(this IBindableCollection<ICommand> commands, CommandFeatures features) {
            return commands.Where((command) => command.HasFeatures(features));
        }
        
        public static IBindableCollection<ICommandFactory> WithFeatures(this IBindableCollection<ICommandFactory> commands, CommandFeatures features) {
            return commands.Where((command) => command.HasFeatures(features));
        }
    }
}