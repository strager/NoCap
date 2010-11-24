using System.Runtime.Serialization;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    [DataContract]
    public class FeaturedCommandProxy : CommandProxy {
        [DataMember]
        private readonly FeaturedCommandCollection commandCollection;

        [DataMember]
        private readonly CommandFeatures features;

        public override ICommand InnerCommand {
            get {
                return this.commandCollection[this.features];
            }

            set {
                this.commandCollection[this.features] = value;
            }
        }

        public FeaturedCommandProxy(FeaturedCommandCollection commandCollection, CommandFeatures features) {
            this.commandCollection = commandCollection;
            this.features = features;
        }
    }
}