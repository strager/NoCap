using System.Windows;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Editors {
    /// <summary>
    /// Interaction logic for ImageFileUploaderEditor.xaml
    /// </summary>
    public partial class ImageFileUploaderEditor : ICommandEditor {
        public readonly static DependencyProperty CommandProperty;
        
        public ImageFileUploader Command {
            get { return (ImageFileUploader) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        static ImageFileUploaderEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ImageFileUploader),
                typeof(ImageFileUploaderEditor)
            );
        }

        public ImageFileUploaderEditor(ImageFileUploader command) {
            InitializeComponent();

            Command = command;
        }
    }
}
