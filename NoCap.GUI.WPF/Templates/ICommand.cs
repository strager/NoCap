using NoCap.Library;

namespace NoCap.GUI.WPF.Templates {
    public interface ICommand : ISource, WinputDotNet.ICommand {
        string Name { get; set; }

        ICommand Clone();

        ICommandFactory GetFactory();
    }
}
