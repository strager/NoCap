using System.Collections.Generic;
using NoCap.Library;

namespace NoCap.GUI.WPF.Commands {
    /// <summary>
    /// Interaction logic for ClipboardUploaderCommandEditor.xaml
    /// </summary>
    public partial class ClipboardUploaderCommandEditor : ICommandEditor {
        private readonly ClipboardUploaderCommand highLevelCommand;

        public ClipboardUploaderCommand HighLevelCommand {
            get {
                return this.highLevelCommand;
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

        public ClipboardUploaderCommandEditor(ClipboardUploaderCommand highLevelCommand) {
            InitializeComponent();

            this.highLevelCommand = highLevelCommand;

            DataContext = this;
        }
    }
}
