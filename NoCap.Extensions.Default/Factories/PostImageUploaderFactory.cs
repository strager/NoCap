using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using NoCap.Extensions.Default.Commands;
using NoCap.Extensions.Default.Editors;
using NoCap.Library;
using NoCap.Library.Imaging;

namespace NoCap.Extensions.Default.Factories {
    [Export(typeof(ICommandFactory))]
    class PostImageUploaderFactory : ICommandFactory {
        public string Name {
            get {
                return "postimage.org uploader";
            }
        }

        public ICommand CreateCommand() {
            return new PostImageUploader(new ImageWriter { Codec = new PngBitmapCodec() });
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new PostImageUploaderEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.ImageUploader;
            }
        }
    }
}
