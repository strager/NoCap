using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for ProviderEditor.xaml
    /// </summary>
    public partial class ProviderEditor {
        public ProviderCollections ProviderCollections {
            get;
            private set;
        }

        public ProviderModules ProviderModules {
            get;
            set;
        }

        public ProviderEditor(ProviderCollections collections, ProviderModules modules) {
            InitializeComponent();

            ProviderCollections = collections;
            ProviderModules = modules;

            DataContext = this;
        }
    }
}
