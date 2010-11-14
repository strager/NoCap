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
        public readonly static DependencyProperty InfoStuffProperty;
        
        public ClipboardUploaderCommand Command {
            get { return (ClipboardUploaderCommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public IInfoStuff InfoStuff {
            get { return (IInfoStuff) GetValue(InfoStuffProperty); }
            set { SetValue(InfoStuffProperty, value); }
        }

        static ClipboardUploaderCommandEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ClipboardUploaderCommand),
                typeof(ClipboardUploaderCommandEditor)
            );

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(IInfoStuff),
                typeof(ClipboardUploaderCommandEditor)
            );
        }

        public ClipboardUploaderCommandEditor(ClipboardUploaderCommand command, IInfoStuff infoStuff) {
            InitializeComponent();

            Command = command;
            InfoStuff = infoStuff;

            this.imageUploaderEditor.AutoLoad();
            this.textUploaderEditor.AutoLoad();
            this.urlShortenerEditor.AutoLoad();
        }
    }
}
