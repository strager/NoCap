﻿using System;
using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Imaging;

namespace NoCap.Library.Editors {
    /// <summary>
    /// Interaction logic for ImageWriterEditor.xaml
    /// </summary>
    public partial class ImageWriterEditor : ICommandEditor {
        private readonly HashSet<BitmapCodecFactory> codecFactories;

        public IEnumerable<BitmapCodecFactory> CodecFactories {
            get {
                return this.codecFactories;
            }
        }

        public ImageWriterEditor(IInfoStuff infoStuff) :
            this(infoStuff.GetBitmapCodecFactories()) {
        }

        public ImageWriterEditor(IEnumerable<BitmapCodecFactory> codecFactories) {
            // Must be set before the InitializeCompoent call
            // so bindings are set up against these (and not null)
            this.codecFactories = new HashSet<BitmapCodecFactory>(codecFactories);

            InitializeComponent();

            this.codecSelector.Filter = new Predicate<ICommandFactory>((factory) => this.codecFactories.Contains(factory));
        }
    }
}
