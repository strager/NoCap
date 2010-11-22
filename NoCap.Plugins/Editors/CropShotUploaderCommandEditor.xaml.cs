using System.Linq;
using System.Windows;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Editors {
    /// <summary>
    /// Interaction logic for CropShotUploaderCommandEditor.xaml
    /// </summary>
    public partial class CropShotUploaderCommandEditor : ICommandEditor {
        public readonly static DependencyProperty CommandProperty;
        
        public CropShotUploaderCommand Command {
            get { return (CropShotUploaderCommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        static CropShotUploaderCommandEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(CropShotUploaderCommand),
                typeof(CropShotUploaderCommandEditor)
            );
        }

        public CropShotUploaderCommandEditor(CropShotUploaderCommand command) {
            InitializeComponent();

            Command = command;

            this.imageUploaderEditor.AutoLoad();
        }
    }
}
