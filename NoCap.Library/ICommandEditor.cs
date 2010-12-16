using System.Windows;

namespace NoCap.Library {
    // TODO Should inheriting from UIElement/FrameworkElement be enforced?
    /// <summary>
    /// Represents a GUI editor of an instance of <see cref="ICommand"/>.
    /// </summary>
    /// <remarks>
    /// Typically, implementors should implement this interface while
    /// inheriting <see cref="System.Windows.Controls.ContentControl"/>.
    /// The editor instance is inserted into a GUI as if it was a normal control.
    /// The <see cref="FrameworkElement.DataContext"/> of the editor is set
    /// to the command to be edited.
    /// </remarks>
    public interface ICommandEditor {
    }
}