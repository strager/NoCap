using System.Windows.Controls;

namespace NoCap.Library {
    public interface IProcessorFactory : INamedComponent {
        IProcessor CreateProcessor();

        ContentControl GetProcessorEditor(IProcessor processor);
    }
}
