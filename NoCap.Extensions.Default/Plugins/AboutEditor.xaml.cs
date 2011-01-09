using System.Diagnostics;
using System.Windows.Navigation;

namespace NoCap.Extensions.Default.Plugins {
    /// <summary>
    /// Interaction logic for AboutEditor.xaml
    /// </summary>
    public partial class AboutEditor {
        internal AboutEditor() {
            InitializeComponent();
        }

        private void NavigateBrowser(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }
    }
}
