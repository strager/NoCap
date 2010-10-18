using System.Windows.Controls;
using WinputDotNet;

namespace NoCap.GUI.WPF.Templates {
    public interface ICommandFactory : INamedComponent {
        ICommand CreateTemplate();

        ContentControl GetCommandEditor(ICommand command);
    }
}
