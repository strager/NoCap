using System.Collections.Generic;
using NoCap.Library.Processors;

namespace NoCap.GUI.WPF.Commands {
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

        public IEnumerable<ImageUploader> ImageUploaders {
            get;
            set;
        }

        public IEnumerable<TextUploader> TextUploaders {
            get;
            set;
        }

        public IEnumerable<UrlShortener> UrlShorteners {
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
