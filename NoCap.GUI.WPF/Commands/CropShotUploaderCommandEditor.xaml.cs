using System.Collections.Generic;
using NoCap.Library;

namespace NoCap.GUI.WPF.Commands {
    /// <summary>
    /// Interaction logic for CropShotUploaderCommandEditor.xaml
    /// </summary>
    public partial class CropShotUploaderCommandEditor : IProcessorEditor {
        private readonly CropShotUploaderCommand command;

        public CropShotUploaderCommand Command {
            get {
                return this.command;
            }
        }

        public IEnumerable<IProcessor> ImageUploaders {
            get;
            set;
        }

        public CropShotUploaderCommandEditor(CropShotUploaderCommand command) {
            InitializeComponent();

            this.command = command;

            DataContext = this;
        }
    }
}
