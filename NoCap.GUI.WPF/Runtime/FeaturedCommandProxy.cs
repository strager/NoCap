using System;
using System.Runtime.Serialization;
using NoCap.GUI.WPF.Settings;
using NoCap.Library;

namespace NoCap.GUI.WPF.Runtime {
    [DataContract(Name = "FeaturedCommandProxy")]
    public class FeaturedCommandProxy : CommandProxy, IExtensibleDataObject {
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

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}