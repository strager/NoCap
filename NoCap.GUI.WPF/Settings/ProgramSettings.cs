using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using NoCap.Library;
using WinputDotNet;
using NoCap.Library.Util;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF.Settings {
    [Serializable]
    public sealed class ProgramSettings : ISerializable {
        public IInputProvider InputProvider {
            get;
            set;
        }

        public ObservableCollection<StandAloneCommandBinding> Bindings {
            get;
            set;
        }

        public ObservableCollection<ICommand> Commands {
            get;
            set;
        }

        private readonly IInfoStuff infoStuff;

        public IInfoStuff InfoStuff {
            get {
                return this.infoStuff;
            }
        }

        public ProgramSettings() :
            this(Providers.Instance) {
        }

        public ProgramSettings(Providers providers) {
            InputProvider = providers.InputProviders.FirstOrDefault();
            Bindings = new ObservableCollection<StandAloneCommandBinding>();
            Commands = new ObservableCollection<ICommand>();

            this.infoStuff = new ProgramSettingsInfoStuff(this, providers);
        }

        public ProgramSettings(SerializationInfo info, StreamingContext context) {
            var providers = Providers.Instance;

            Bindings = info.GetValue<ObservableCollection<StandAloneCommandBinding>>("Bindings");
            Commands = info.GetValue<ObservableCollection<ICommand>>("Commands");

            var inputProviderType = info.GetValue<Type>("InputProvider type");

            var inputProvider =
                providers.InputProviders.FirstOrDefault((provider) => provider.GetType().Equals(inputProviderType))
                ?? providers.InputProviders.FirstOrDefault();

            InputProvider = inputProvider;

            this.infoStuff = new ProgramSettingsInfoStuff(this, providers);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Bindings", Bindings);
            info.AddValue("Commands", Commands);

            info.AddValue("InputProvider type", InputProvider.GetType());
        }
    }
}
