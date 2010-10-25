using System.Collections.Generic;
using NoCap.Library;

namespace NoCap.GUI.WPF.Commands {
    /// <summary>
    /// Interaction logic for CropShotUploaderCommandEditor.xaml
    /// </summary>
    public partial class CropShotUploaderCommandEditor : IProcessorEditor {
        private readonly CropShotUploaderCommand highLevelCommand;

        public IEnumerable<IProcessor> ImageUploaders {
            get;
            set;
        }

        public CropShotUploaderCommand HighLevelCommand {
            get {
                return this.highLevelCommand;
            }
        }

        public CropShotUploaderCommandEditor(CropShotUploaderCommand highLevelCommand) {
            InitializeComponent();

            this.highLevelCommand = highLevelCommand;

            DataContext = this;
        }
    }
}
