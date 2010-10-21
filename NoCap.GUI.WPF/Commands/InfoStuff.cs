using System.Collections.Generic;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Processors;

namespace NoCap.GUI.WPF.Commands {
    public static class InfoStuff {
        public static IEnumerable<IProcessor> GetTextUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Processors.OfType<TextUploader>();
        }
        
        public static IEnumerable<IProcessor> GetImageUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Processors.OfType<ImageUploader>();
        }
        
        public static IEnumerable<IProcessor> GetUrlShorteners(this IInfoStuff infoStuff) {
            return infoStuff.Processors.OfType<UrlShortener>();
        }
    }
}