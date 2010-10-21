namespace NoCap.Library {
    /// <summary>
    /// Represents a GUI editor of an instance of <see cref="IProcessor"/>.
    /// </summary>
    /// <remarks>
    /// Typically, implementors should implement this interface while
    /// inheriting <see cref="System.Windows.Controls.ContentControl"/>.
    /// The editor instance is inserted into a GUI as if it was a normal control.
    /// Other than the insertion, no other operations are performed; setting
    /// the appropriate data context (if necessary) is the responsibility of
    /// implementors.
    /// </remarks>
    public interface IProcessorEditor {
    }
}