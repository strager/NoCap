using System.ComponentModel;
using System.Linq;
using System.Windows;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for ProviderEditor.xaml
    /// </summary>
    public partial class CommandSettingsEditor : INotifyPropertyChanged {
        public static readonly DependencyProperty InfoStuffProperty;

        public string DisplayName {
            get {
                return "Commands";
            }
        }

        static CommandSettingsEditor() {
            InfoStuffProperty = InfoStuffWpf.InfoStuffProperty.AddOwner(typeof(CommandSettingsEditor));
        }

        public CommandSettingsEditor() {
            InitializeComponent();

            SetResourceReference(InfoStuffProperty, "InfoStuff");

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
