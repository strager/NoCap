using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    public class FeaturedCommand {
        private readonly FeaturedCommandCollection commandCollection;
        private readonly CommandFeatures features;

        public CommandFeatures Features {
            get {
                return this.features;
            }
        }

        public ICommand Command {
            get {
                return this.commandCollection[features];
            }

            set {
                this.commandCollection[features] = value;
            }
        }

        public CommandProxy Proxy {
            get {
                return this.commandCollection.GetProxy(features);
            }
        }

        public FeaturedCommand(FeaturedCommandCollection commandCollection, CommandFeatures features) {
            this.commandCollection = commandCollection;
            this.features = features;
        }
    }
}