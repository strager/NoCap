using System;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        private Program program;

        protected override void OnStartup(System.Windows.StartupEventArgs e) {
            base.OnStartup(e);

            this.program = new Program();
            this.program.Run();
        }

        [STAThread]
        static void Main() {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
