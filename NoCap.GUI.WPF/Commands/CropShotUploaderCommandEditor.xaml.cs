using System.Collections.Generic;
using NoCap.Library;
using NoCap.Library.Processors;

namespace NoCap.GUI.WPF.Commands {
    /// <summary>
    /// Interaction logic for CropShotUploaderCommandEditor.xaml
    /// </summary>
    public partial class CropShotUploaderCommandEditor : ICommandEditor {
        private readonly CropShotUploaderCommand command;

        public IEnumerable<IProcessor> ImageUploaders {
            get;
            set;
        }

        public CropShotUploaderCommand Command {
            get {
                return this.command;
            }
        }

        public CropShotUploaderCommandEditor(CropShotUploaderCommand command) {
            InitializeComponent();

            this.command = command;

            DataContext = this;
        }
    }
}
