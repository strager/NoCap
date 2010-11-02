using System.Collections.Generic;
using System.Drawing.Imaging;
using NoCap.Library.Commands;

namespace NoCap.Library.Editors {
    /// <summary>
    /// Interaction logic for ImageWriterEditor.xaml
    /// </summary>
    public partial class ImageWriterEditor : ICommandEditor {
        private readonly ImageWriter command;
        private readonly IEnumerable<ImageCodecInfo> codecs;

        public ImageWriter Command {
            get {
                return this.command;
            }
        }

        public IEnumerable<ImageCodecInfo> Codecs {
            get {
                return this.codecs;
            }
        }

        public ImageWriterEditor(ImageWriter command) :
            this(command, ImageWriter.DefaultImageCodecs) {
        }

        public ImageWriterEditor(ImageWriter command, IEnumerable<ImageCodecInfo> codecs) {
            // Must be set before the InitializeCompoent call
            // so bindings are set up against these (and not null)
            this.command = command;
            this.codecs = codecs;

            InitializeComponent();
        }
    }
}
