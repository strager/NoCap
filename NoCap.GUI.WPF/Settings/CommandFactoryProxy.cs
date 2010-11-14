using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    class CommandFactoryProxy : ICommandFactory {
        private ICommandFactory innerFactory;

        public CommandFactoryProxy(CommandProxy commandProxy) {
            // TODO Update factory when commandProxy.InnerCommand changes

            this.innerFactory = commandProxy.InnerCommand.GetFactory();
        }

        public string Name {
            get {
                return this.innerFactory.Name;
            }
        }

        public ICommand CreateCommand() {
            return this.innerFactory.CreateCommand();
        }

        public void PopulateCommand(ICommand command, IInfoStuff infoStuff) {
            this.innerFactory.PopulateCommand(command, infoStuff);
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return this.innerFactory.GetCommandEditor(command, infoStuff);
        }

        public CommandFeatures CommandFeatures {
            get {
                return this.innerFactory.CommandFeatures;
            }
        }
    }
}