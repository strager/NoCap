using NoCap.Library;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for DefaultCommandsEditor.xaml
    /// </summary>
    public partial class DefaultCommandsEditor {
        public IInfoStuff InfoStuff {
            get;
            set;
        }

        public FeaturedCommandCollection DefaultCommands {
            get;
            set;
        }

        public DefaultCommandsEditor(IInfoStuff infoStuff, FeaturedCommandCollection defaultCommands) {
            InfoStuff = infoStuff;
            DefaultCommands = defaultCommands;

            InitializeComponent();
        }
    }
}
