using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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
using NoCap.Library.Processors;

namespace NoCap.Library.Editors {
    /// <summary>
    /// Interaction logic for ImageWriterEditor.xaml
    /// </summary>
    public partial class ImageWriterEditor : IProcessorEditor {
        private readonly ImageWriter processor;
        private readonly IEnumerable<ImageCodecInfo> codecs;

        public ImageWriter Processor {
            get {
                return this.processor;
            }
        }

        public IEnumerable<ImageCodecInfo> Codecs {
            get {
                return this.codecs;
            }
        }

        public ImageWriterEditor(ImageWriter processor) :
            this(processor, ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid)) {
        }

        public ImageWriterEditor(ImageWriter processor, IEnumerable<ImageCodecInfo> codecs) {
            InitializeComponent();

            this.processor = processor;
            this.codecs = codecs;

            DataContext = this;
        }
    }
}
