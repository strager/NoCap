using System.ComponentModel;
using System.Linq;
using System.Windows;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for ProviderEditor.xaml
    /// </summary>
    public partial class CommandSettingsEditor : INotifyPropertyChanged {
        public static readonly DependencyProperty CommandProviderProperty;

        public string DisplayName {
            get {
                return "Commands";
            }
        }

        static CommandSettingsEditor() {
            CommandProviderProperty = CommandProviderWpf.CommandProviderProperty.AddOwner(typeof(CommandSettingsEditor));
        }

        public CommandSettingsEditor() {
            InitializeComponent();

            SetResourceReference(CommandProviderProperty, "commandProvider");

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
