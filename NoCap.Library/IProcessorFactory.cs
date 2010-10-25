namespace NoCap.Library {
    /// <summary>
    /// Represents a factory which can produce instances of processors
    /// and editors of those processor instances.
    /// </summary>
    public interface IProcessorFactory : INamedComponent {
        /// <summary>
        /// Creates a new processor instance.
        /// </summary>
        /// <param name="infoStuff"></param>
        /// <returns>A new processor instance.</returns>
        IProcessor CreateProcessor(IInfoStuff infoStuff);

        /// <summary>
        /// Gets an editor which can be used to edit the given processor
        /// instance, or <c>null</c> if no editor is needed or can be provided.
        /// </summary>
        /// <param name="processor">The processor to edit.</param>
        /// <param name="infoStuff"></param>
        /// <returns>A new instance of an editor for <paramref name="processor"/>, or <c>null</c>.</returns>
        IProcessorEditor GetProcessorEditor(IProcessor processor, IInfoStuff infoStuff);
    }
}
