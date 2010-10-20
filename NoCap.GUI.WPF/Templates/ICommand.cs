using NoCap.Library.Util;

namespace NoCap.GUI.WPF.Templates {
    public interface ICommand : WinputDotNet.ICommand {
        void Execute(IMutableProgressTracker progress);

        ICommand Clone();

        ICommandFactory GetFactory();
    }
}
