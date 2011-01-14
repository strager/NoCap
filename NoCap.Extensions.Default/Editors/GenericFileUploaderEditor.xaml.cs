using System;
using System.Windows.Controls;
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
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, DeleteParameter));
        }

        private void AddNewParameter(object sender, ExecutedRoutedEventArgs e) {
            Command.PostParameters.Add(new StringPair(null, ""));
        }

        private void DeleteParameter(object sender, ExecutedRoutedEventArgs e) {
            var kvp = (StringPair) e.Parameter;

            Command.PostParameters.Remove(kvp);
        }

        private void DisableSelection(object sender, SelectionChangedEventArgs e) {
            this.parameterEditor.SelectedIndex = -1;
        }
    }
}
