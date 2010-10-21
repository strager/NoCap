namespace NoCap.Library {
    public interface IProcessorFactory : INamedComponent {
        IProcessor CreateProcessor();

        IProcessorEditor GetProcessorEditor(IProcessor processor);
    }
}
