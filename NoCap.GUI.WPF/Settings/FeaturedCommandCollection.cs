using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    [DataContract(Name = "FeaturedCommands")]
    public class FeaturedCommandCollection {
        [DataMember]
        private readonly IDictionary<CommandFeatures, ICommand> commands = new Dictionary<CommandFeatures, ICommand>();

        [IgnoreDataMember]
        private IDictionary<CommandFeatures, string> names = new Dictionary<CommandFeatures, string>();

        [IgnoreDataMember]
        private IDictionary<CommandFeatures, CommandProxy> proxies = new Dictionary<CommandFeatures, CommandProxy>();

        internal IDictionary<CommandFeatures, ICommand> Commands {
            get {
                return this.commands;
            }
        }

        internal IDictionary<CommandFeatures, string> Names {
            get {
                return this.names;
            }
        }

        public IEnumerable<FeaturedCommand> RegisteredCommands {
            get {
                return this.names.Keys.Select(Get);
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) {
            this.names = new Dictionary<CommandFeatures, string>();
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

        public void RegisterDefaultType(CommandFeatures features, string name, IInfoStuff infoStuff) {
            if (this.names.ContainsKey(features)) {
                throw new InvalidOperationException("Default type already registered");
            }

            var preferredCommand = infoStuff.GetPreferredCommand(features);

            if (preferredCommand == null) {
                throw new InvalidOperationException("There must be a command for the given features to register a default type");
            }

            this.names[features] = name;

            if (!this.commands.ContainsKey(features)) {
                this.commands[features] = preferredCommand;
            }
        }

        public bool Contains(ICommand command) {
            return command != null && command is FeaturedCommandProxy;
        }
    }
}