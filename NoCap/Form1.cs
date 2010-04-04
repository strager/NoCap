using System;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace NoCap {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            var img = ScreenCapturer.CaptureEntireDesktop();
            img.Save(@"img.bmp", ImageFormat.Bmp);
            MessageBox.Show("done");
        }
    }
}
