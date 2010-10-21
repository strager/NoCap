using INamedComponent = WinputDotNet.INamedComponent;

namespace NoCap.GUI.WPF.Commands {
    public interface ICommandFactory : INamedComponent {
        ICommand CreateCommand(IInfoStuff infoStuff);

        ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff);
    }
}
