using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow {
        public SettingsWindow() {
            InitializeComponent();

            var tab = new TabItem {
                Header = "Providers"
            };

            var providerCollections = new ProviderCollections();
            tab.Content = new ProviderEditor(providerCollections, providerCollections.GetDefaultModules());

            this.tabControl.Items.Add(tab);
        }
    }
}
