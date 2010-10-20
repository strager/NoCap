using System.Collections.ObjectModel;
using NoCap.Library;

namespace NoCap.GUI.WPF.Commands {
    /// <summary>
    /// Interaction logic for CropShotUploaderCommandEditor.xaml
    /// </summary>
    public partial class CropShotUploaderCommandEditor : ICommandEditor {
        public CropShotUploaderCommandEditor() {
            InitializeComponent();
        }

        public ObservableCollection<IProcessor> Processors {
            get;
            set;
        }
    }
}
