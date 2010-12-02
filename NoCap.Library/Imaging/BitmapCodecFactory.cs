namespace NoCap.Library.Imaging {
    public abstract class BitmapCodecFactory : ICommandFactory {
        public abstract string Name { get; }

        ICommand ICommandFactory.CreateCommand() {
            return CreateCommand();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public abstract BitmapCodec CreateCommand();

        public abstract ICommandEditor GetCommandEditor(ICommandProvider commandProvider);

        public virtual CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.None;
            }
        }
    }
}