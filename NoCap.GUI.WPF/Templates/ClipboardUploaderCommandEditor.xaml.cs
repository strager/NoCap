using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Destinations;

namespace NoCap.GUI.WPF.Templates {
    /// <summary>
    /// Interaction logic for ClipboardUploaderCommandEditor.xaml
    /// </summary>
    public partial class ClipboardUploaderCommandEditor {
        private readonly ClipboardUploaderCommand command;
        private readonly Providers providers;

        public ClipboardUploaderCommand Command {
            get {
                return this.command;
            }
        }

        public IEnumerable<ImageUploader> ImageUploaders {
            get {
                return this.providers.Processors.OfType<ImageUploader>();
            }
        }

        public IEnumerable<TextUploader> TextUploaders {
            get {
                return this.providers.Processors.OfType<TextUploader>();
            }
        }

        public IEnumerable<UrlShortener> UrlShorteners {
            get {
                return this.providers.Processors.OfType<UrlShortener>();
            }
        }

        public ClipboardUploaderCommandEditor(ClipboardUploaderCommand command) {
            InitializeComponent();

            this.command = command;
            this.providers = Providers.Instance;

            DataContext = this;
        }
    }
}
