using System.Windows;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Editors {
    /// <summary>
    /// Interaction logic for ImageFileUploaderEditor.xaml
    /// </summary>
    public partial class ImageFileUploaderEditor : ICommandEditor {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty InfoStuffProperty;
        
        public ImageFileUploader Command {
            get { return (ImageFileUploader) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public IInfoStuff InfoStuff {
            get { return (IInfoStuff) GetValue(InfoStuffProperty); }
            set { SetValue(InfoStuffProperty, value); }
        }

        static ImageFileUploaderEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ImageFileUploader),
                typeof(ImageFileUploaderEditor)
            );

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(IInfoStuff),
                typeof(ImageFileUploaderEditor)
            );
        }

        public ImageFileUploaderEditor(ImageFileUploader command, IInfoStuff infoStuff) {
            InitializeComponent();

            Command = command;
            InfoStuff = infoStuff;

            this.fileUploaderSelector.Filter = Library.Command.GetHasFeaturesPredicate(CommandFeatures.FileUploader);
        }
    }
}
