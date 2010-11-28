using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NoCap.Library;
using NoCap.Library.Controls;

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
            CommandProviderProperty = NoCapControl.CommandProviderProperty.AddOwner(typeof(CommandSettingsEditor));
        }

        public CommandSettingsEditor() {
            InitializeComponent();

            // Can't easily be set in XAML
            // TODO Move to XAML
            SetBinding(VisibilityProperty, new Binding {
                Path = new PropertyPath(NoCapControl.ShowAdvancedProperty),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                Converter = new BooleanToVisibilityConverter()
            });

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
