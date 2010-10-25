using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Commands;

namespace NoCap.Library {
    public static class InfoStuff {
        public static IEnumerable<ICommand> GetTextUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Commands.OfType<TextUploader>();
        }
        
        public static IEnumerable<ICommand> GetImageUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Commands.OfType<ImageUploader>();
        }
        
        public static IEnumerable<ICommand> GetUrlShorteners(this IInfoStuff infoStuff) {
            return infoStuff.Commands.OfType<UrlShortener>();
        }
    }
}