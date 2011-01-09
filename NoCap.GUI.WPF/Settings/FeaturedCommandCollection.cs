using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NoCap.GUI.WPF.Runtime;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    [DataContract(Name = "FeaturedCommands")]
    public class FeaturedCommandCollection : IExtensibleDataObject {
        [DataMember]
        private readonly IDictionary<CommandFeatures, ICommand> commands = new Dictionary<CommandFeatures, ICommand>();

        [IgnoreDataMember]
        private IDictionary<CommandFeatures, CommandProxy> proxies = new Dictionary<CommandFeatures, CommandProxy>();

        internal IDictionary<CommandFeatures, ICommand> Commands {
            get {
                return this.commands;
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) {
            this.proxies = new Dictionary<CommandFeatures, CommandProxy>();
        }

        public ICommand this[CommandFeatures features] {
            get {
                return Commands[features];
            }

            set {
                Commands[features] = value;
            }
        }

        public void Clear() {
            Commands.Clear();
        }

        public CommandProxy GetProxy(CommandFeatures features) {
            CommandProxy proxy;

            if (this.proxies.TryGetValue(features, out proxy)) {
                return proxy;
            }

            proxy = new FeaturedCommandProxy(this, features);
            this.proxies[features] = proxy;

            return proxy;
        }

        public bool Contains(ICommand command) {
            return command != null && command is FeaturedCommandProxy;
        }

        public bool ContainsKey(CommandFeatures features) {
            return this.commands.ContainsKey(features);
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}