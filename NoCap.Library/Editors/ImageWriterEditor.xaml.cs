using System;
using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Commands.Imaging;

namespace NoCap.Library.Editors {
    /// <summary>
    /// Interaction logic for ImageWriterEditor.xaml
    /// </summary>
    public partial class ImageWriterEditor : ICommandEditor {
        private readonly ImageWriter command;
        private readonly IInfoStuff infoStuff;
        private readonly HashSet<BitmapCodecFactory> codecFactories;

        public ImageWriter Command {
            get {
                return this.command;
            }
        }

        public IInfoStuff InfoStuff {
            get {
                return this.infoStuff;
            }
        }

        public IEnumerable<BitmapCodecFactory> CodecFactories {
            get {
                return this.codecFactories;
            }
        }

        public ImageWriterEditor(ImageWriter command, IInfoStuff infoStuff) :
            this(command, infoStuff, infoStuff.GetBitmapCodecFactories()) {
        }

        public ImageWriterEditor(ImageWriter command, IInfoStuff infoStuff, IEnumerable<BitmapCodecFactory> codecFactories) {
            // Must be set before the InitializeCompoent call
            // so bindings are set up against these (and not null)
            this.command = command;
            this.infoStuff = infoStuff;
            this.codecFactories = new HashSet<BitmapCodecFactory>(codecFactories);

            InitializeComponent();

            this.codecSelector.Filter = new Predicate<ICommandFactory>((factory) => this.codecFactories.Contains(factory));
        }
    }
}
