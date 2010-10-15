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
    public partial class ProviderEditor : ISettingsEditor {
        public Providers Providers {
            get;
            private set;
        }

        public Settings Settings {
            get;
            private set;
        }

        public string DisplayName {
            get {
                return "Providers";
            }
        }

        public ProviderEditor(Settings settings) {
            InitializeComponent();
            
            Settings = settings;
            Providers = Providers.Instance;

            DataContext = this;
        }
    }

    public interface ISettingsEditor {
        string DisplayName {
            get;
        }
    }
}
