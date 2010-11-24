using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library.Imaging {
    static class CommandProviderExtensions {
        public static IEnumerable<BitmapCodecFactory> GetBitmapCodecFactories(this ICommandProvider commandProvider) {
            return commandProvider.CommandFactories.OfType<BitmapCodecFactory>();
        }
    }
}
