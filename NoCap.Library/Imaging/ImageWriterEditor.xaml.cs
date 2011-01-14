using System;

namespace NoCap.Library.Imaging {
    /// <summary>
    /// Interaction logic for ImageWriterEditor.xaml
    /// </summary>
    public partial class ImageWriterEditor : ICommandEditor {
        public ImageWriterEditor() {
            InitializeComponent();

            this.codecSelector.Filter = new Predicate<ICommandFactory>((factory) => factory is BitmapCodecFactory);
        }
    }
}
