using System.Windows.Controls;
using WinputDotNet;

namespace NoCap.GUI.WPF.Commands {
    public interface ICommandFactory : INamedComponent {
        ICommand CreateTemplate();

        ContentControl GetCommandEditor(ICommand command);
    }
}
