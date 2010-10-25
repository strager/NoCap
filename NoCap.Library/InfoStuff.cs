using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Commands;

namespace NoCap.Library {
    public static class InfoStuff {
        public static IEnumerable<ICommand> GetTextUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Processors.OfType<TextUploader>();
        }
        
        public static IEnumerable<ICommand> GetImageUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Processors.OfType<ImageUploader>();
        }
        
        public static IEnumerable<ICommand> GetUrlShorteners(this IInfoStuff infoStuff) {
            return infoStuff.Processors.OfType<UrlShortener>();
        }
    }
}