using System.Collections.ObjectModel;
using NoCap.Library;
using INamedComponent = WinputDotNet.INamedComponent;

namespace NoCap.GUI.WPF.Commands {
    public interface ICommandFactory : INamedComponent {
        ICommand CreateCommand();

        ICommandEditor GetCommandEditor(ICommand command);
    }

    public interface ICommandEditor {
        ObservableCollection<IProcessor> Processors { get; set; }
    }
}
