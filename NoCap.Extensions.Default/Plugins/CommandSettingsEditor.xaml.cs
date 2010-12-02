using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NoCap.Library.Controls;

namespace NoCap.Extensions.Default.Plugins {
    /// <summary>
    /// Interaction logic for ProviderEditor.xaml
    /// </summary>
    public partial class CommandSettingsEditor {
        public static readonly DependencyProperty CommandProviderProperty;

        public string DisplayName {
            get { return "Commands"; }
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
    }
}
