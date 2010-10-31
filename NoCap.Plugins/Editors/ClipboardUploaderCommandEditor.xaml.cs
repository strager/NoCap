using System.Collections.Generic;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Editors {
    /// <summary>
    /// Interaction logic for ClipboardUploaderCommandEditor.xaml
    /// </summary>
    public partial class ClipboardUploaderCommandEditor : ICommandEditor {
        private readonly ClipboardUploaderCommand command;

        public ClipboardUploaderCommand Command {
            get {
                return this.command;
            }
        }

        // TODO Bindable Linq

        public IEnumerable<ICommand> ImageUploaders {
            get;
            set;
        }

        public IEnumerable<ICommand> TextUploaders {
            get;
            set;
        }

        public IEnumerable<ICommand> UrlShorteners {
            get;
            set;
        }

        public ClipboardUploaderCommandEditor(ClipboardUploaderCommand command) {
            InitializeComponent();

            this.command = command;

            DataContext = this;
        }
    }
}
