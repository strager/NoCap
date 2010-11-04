namespace NoCap.Library.Commands.Imaging {
    /// <summary>
    /// Interaction logic for JpegBitmapCodecEditor.xaml
    /// </summary>
    public partial class JpegBitmapCodecEditor : ICommandEditor {
        private readonly JpegBitmapCodec codec;

        public JpegBitmapCodec Codec {
            get {
                return this.codec;
            }
        }

        public JpegBitmapCodecEditor(JpegBitmapCodec codec) {
            this.codec = codec;

            InitializeComponent();
        }
    }
}
