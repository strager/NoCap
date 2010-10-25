using System.Collections.Generic;
using NoCap.Library;

namespace NoCap.GUI.WPF.Commands {
    /// <summary>
    /// Interaction logic for ClipboardUploaderCommandEditor.xaml
    /// </summary>
    public partial class ClipboardUploaderCommandEditor : IProcessorEditor {
        private readonly ClipboardUploaderCommand command;

        public ClipboardUploaderCommand Command {
            get {
                return this.command;
            }
        }

        // TODO Bindable Linq

        public IEnumerable<IProcessor> ImageUploaders {
            get;
            set;
        }

        public IEnumerable<IProcessor> TextUploaders {
            get;
            set;
        }

        public IEnumerable<IProcessor> UrlShorteners {
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
