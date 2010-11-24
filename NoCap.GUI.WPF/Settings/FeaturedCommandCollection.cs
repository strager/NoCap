using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    [DataContract(Name = "FeaturedCommands")]
    public class FeaturedCommandCollection : IEnumerable<FeaturedCommand> {
        [DataMember]
        private readonly IDictionary<CommandFeatures, ICommand> commands = new Dictionary<CommandFeatures, ICommand>();

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

        public IEnumerator<FeaturedCommand> GetEnumerator() {
            return Commands.Keys.Select(Get).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
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

        public FeaturedCommand Get(CommandFeatures features) {
            return new FeaturedCommand(this, features);
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
    }
}