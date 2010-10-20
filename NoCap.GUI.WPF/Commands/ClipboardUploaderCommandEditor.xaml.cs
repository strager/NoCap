using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Processors;

namespace NoCap.GUI.WPF.Commands {
    /// <summary>
    /// Interaction logic for ClipboardUploaderCommandEditor.xaml
    /// </summary>
    public partial class ClipboardUploaderCommandEditor : ICommandEditor {
        private readonly ClipboardUploaderCommand command;

        public ObservableCollection<IProcessor> Processors {
            get;
            set;
        }

        public ClipboardUploaderCommand Command {
            get {
                return this.command;
            }
        }

        // TODO Bindable Linq

        public IEnumerable<ImageUploader> ImageUploaders {
            get {
                return Processors.OfType<ImageUploader>();
            }
        }

        public IEnumerable<TextUploader> TextUploaders {
            get {
                return Processors.OfType<TextUploader>();
            }
        }

        public IEnumerable<UrlShortener> UrlShorteners {
            get {
                return Processors.OfType<UrlShortener>();
            }
        }

        public ClipboardUploaderCommandEditor(ClipboardUploaderCommand command) {
            InitializeComponent();

            this.command = command;

            DataContext = this;
        }
    }
}
