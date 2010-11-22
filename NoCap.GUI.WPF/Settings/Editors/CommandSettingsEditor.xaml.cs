using System.ComponentModel;
using System.Linq;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for ProviderEditor.xaml
    /// </summary>
    public partial class CommandSettingsEditor : INotifyPropertyChanged {
        private IInfoStuff infoStuff;

        public IInfoStuff InfoStuff {
            get {
                return this.infoStuff;
            }

            set {
                this.infoStuff = value;

                Notify("InfoStuff");
            }
        }

        private ICommand selectedCommand;

        public ICommand SelectedCommand {
            get {
                return this.selectedCommand;
            }

            set {
                this.selectedCommand = value;

                Notify("SelectedCommand");
            }
        }

        public string DisplayName {
            get {
                return "Commands";
            }
        }

        public CommandSettingsEditor(IInfoStuff infoStuff) {
            InitializeComponent();

            InfoStuff = infoStuff;

            this.commandSelector.AutoLoad();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
