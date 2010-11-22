using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Editors {
    /// <summary>
    /// Interaction logic for ClipboardUploaderCommandEditor.xaml
    /// </summary>
    public partial class ClipboardUploaderCommandEditor : ICommandEditor {
        public readonly static DependencyProperty CommandProperty;
        
        public ClipboardUploaderCommand Command {
            get { return (ClipboardUploaderCommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        static ClipboardUploaderCommandEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ClipboardUploaderCommand),
                typeof(ClipboardUploaderCommandEditor)
            );
        }

        public ClipboardUploaderCommandEditor(ClipboardUploaderCommand command) {
            InitializeComponent();

            Command = command;

            this.imageUploaderEditor.AutoLoad();
            this.textUploaderEditor.AutoLoad();
            this.urlShortenerEditor.AutoLoad();
        }
    }
}
