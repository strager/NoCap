using System.Windows.Input;
using NoCap.Extensions.Default.Commands;
using NoCap.Library;

namespace NoCap.Extensions.Default.Editors {
    /// <summary>
    /// Interaction logic for GenericFileUploaderEditor.xaml
    /// </summary>
    public partial class GenericFileUploaderEditor : ICommandEditor {
        private GenericFileUploader Command {
            get {
                return DataContext as GenericFileUploader;
            }
        }

        public GenericFileUploaderEditor() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, AddNewParameter));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, DeleteParameter, CanDeleteParameter));
        }

        private void AddNewParameter(object sender, ExecutedRoutedEventArgs e) {
            Command.PostParameters.Add(new StringPair(null, ""));
        }

        private void DeleteParameter(object sender, ExecutedRoutedEventArgs e) {
            var kvp = (StringPair) this.parameterEditor.SelectedItem;

            Command.PostParameters.Remove(kvp);
        }

        private void CanDeleteParameter(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = this.parameterEditor.SelectedIndex >= 0;
            e.Handled = true;
        }
    }
}
