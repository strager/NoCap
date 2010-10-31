using System.Windows;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Editors {
    /// <summary>
    /// Interaction logic for CropShotUploaderCommandEditor.xaml
    /// </summary>
    public partial class CropShotUploaderCommandEditor : ICommandEditor {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty InfoStuffProperty;
        
        public CropShotUploaderCommand Command {
            get { return (CropShotUploaderCommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public IInfoStuff InfoStuff {
            get { return (IInfoStuff) GetValue(InfoStuffProperty); }
            set { SetValue(InfoStuffProperty, value); }
        }

        static CropShotUploaderCommandEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(CropShotUploaderCommand),
                typeof(CropShotUploaderCommandEditor)
            );

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(CropShotUploaderCommand),
                typeof(IInfoStuff)
            );
        }

        public CropShotUploaderCommandEditor(CropShotUploaderCommand command, IInfoStuff infoStuff) {
            InitializeComponent();

            this.imageUploaderSelector.InfoStuff = infoStuff;

            Command = command;
        }
    }
}
