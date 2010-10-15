using System;
using System.Collections;
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
using WinputDotNet;

namespace NoCap.GUI.WPF.Settings {
    /// <summary>
    /// Interaction logic for BindingSettingEditor.xaml
    /// </summary>
    public partial class BindingSettingEditor : ISettingsEditor {
        public string DisplayName {
            get {
                return "Bindings";
            }
        }

        public ProgramSettings ProgramSettings {
            get;
            private set;
        }

        public BindingSettingEditor(ProgramSettings settings) {
            InitializeComponent();

            ProgramSettings = settings;

            DataContext = this;
        }

        private void AddBindingClicked(object sender, RoutedEventArgs e) {
            ProgramSettings.Bindings.Add(new SourceDestinationCommandBinding(null, null));
        }

        private void SetBindingClicked(object sender, RoutedEventArgs e) {
            // Hackish way to get the binding from the hyperlink
            var hyperlink = (Hyperlink) sender;
            var binding = (SourceDestinationCommandBinding) hyperlink.DataContext;

            IInputSequence inputSequence;

            if (TryGetInputSequence(out inputSequence)) {
                binding.Input = inputSequence;
            }
        }

        private bool TryGetInputSequence(out IInputSequence inputSequence) {
            var window = new BindWindow(ProgramSettings.InputProvider);

            if (window.ShowDialog() == true) {
                inputSequence = window.InputSequence;

                return true;
            } else {
                inputSequence = null;

                return false;
            }
        }
    }
}
