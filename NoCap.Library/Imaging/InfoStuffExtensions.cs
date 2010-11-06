using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library.Imaging {
    public static class InfoStuffExtensions {
        public static IEnumerable<BitmapCodecFactory> GetBitmapCodecFactories(this IInfoStuff infoStuff) {
            return infoStuff.CommandFactories.OfType<BitmapCodecFactory>();
        }
    }
}
