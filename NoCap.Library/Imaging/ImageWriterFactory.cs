﻿namespace NoCap.Library.Imaging {
    public class ImageWriterFactory : ICommandFactory {
        public string Name {
            get {
                return "Image writer";
            }
        }

        public ICommand CreateCommand() {
            return new ImageWriter(new PngBitmapCodec());
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new ImageWriterEditor(commandProvider);
        }

        public CommandFeatures CommandFeatures {
            get {
                return 0;
            }
        }
    }
}