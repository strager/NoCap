using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Processors;
using INamedComponent = WinputDotNet.INamedComponent;

namespace NoCap.GUI.WPF.Commands {
    public interface ICommandFactory : INamedComponent {
        ICommand CreateCommand(IInfoStuff infoStuff);

        ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff);
    }

    public interface IInfoStuff {
        ObservableCollection<IProcessor> Processors { get; }
    }

    public static class InfoStuff {
        public static IEnumerable<TextUploader> GetTextUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Processors.OfType<TextUploader>();
        }
        
        public static IEnumerable<ImageUploader> GetImageUploaders(this IInfoStuff infoStuff) {
            return infoStuff.Processors.OfType<ImageUploader>();
        }
        
        public static IEnumerable<UrlShortener> GetUrlShorteners(this IInfoStuff infoStuff) {
            return infoStuff.Processors.OfType<UrlShortener>();
        }
    }

    public interface ICommandEditor {
    }
}
