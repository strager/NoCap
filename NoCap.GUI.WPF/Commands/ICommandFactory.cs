using INamedComponent = WinputDotNet.INamedComponent;

namespace NoCap.GUI.WPF.Commands {
    public interface ICommandFactory : INamedComponent {
        HighLevelCommand CreateCommand(IInfoStuff infoStuff);

        ICommandEditor GetCommandEditor(HighLevelCommand highLevelCommand, IInfoStuff infoStuff);
    }
}
