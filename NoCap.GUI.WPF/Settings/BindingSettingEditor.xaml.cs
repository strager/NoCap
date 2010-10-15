using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

        private SourceDestinationCommandBinding SelectedBinding {
            get {
                return (SourceDestinationCommandBinding) this.bindingsList.SelectedItem;
            }
        }

        public BindingSettingEditor(ProgramSettings settings) {
            InitializeComponent();

            ProgramSettings = settings;

            DataContext = this;
        }

        private void AddBindingClicked(object sender, RoutedEventArgs e) {
            IInputSequence inputSequence;

            if (!TryGetInputSequence(out inputSequence)) {
                return;
            }

            var binding = new SourceDestinationCommandBinding(null, null) {
                Input = inputSequence
            };

            ProgramSettings.Bindings.Add(binding);
        }

        private void DeleteBindingClicked(object sender, RoutedEventArgs e) {
            if (SelectedBinding != null) {
                ProgramSettings.Bindings.Remove(SelectedBinding);
            }
        }

        private void ChangeBindingClicked(object sender, RoutedEventArgs e) {
            if (SelectedBinding != null) {
                ChangeBinding(SelectedBinding);
            }
        }

        private void ChangeBinding(SourceDestinationCommandBinding binding) {
            if (binding == null) {
                throw new ArgumentNullException("binding");
            }

            IInputSequence inputSequence;

            if (!TryGetInputSequence(out inputSequence)) {
                return;
            }

            // We use this instead of direct assignment to inform binders
            // of the update
            var properties = TypeDescriptor.GetProperties(binding);
            var inputProperty = properties.Find("Input", false);
            inputProperty.SetValue(binding, inputSequence);
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

    public class NullableBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
