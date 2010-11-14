using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    public class FeaturedCommandCollection : IEnumerable<FeaturedCommand> {
        private readonly IDictionary<CommandFeatures, FeaturedCommand> commands = new Dictionary<CommandFeatures, FeaturedCommand>();

        public IEnumerator<FeaturedCommand> GetEnumerator() {
            return this.commands.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public ICommand this[CommandFeatures features] {
            get {
                var featuredCommand = this.commands[features];

                return featuredCommand == null ? null : featuredCommand.Command;
            }

            set {
                this.commands[features] = new FeaturedCommand(features, value);
            }
        }

        public void Add(FeaturedCommand item) {
            this.commands.Add(item.Features, item);
        }

        public void Clear() {
            this.commands.Clear();
        }

        public FeaturedCommand Get(CommandFeatures features) {
            return this.commands[features];
        }

        public bool Contains(ICommand command) {
            return this.Any((featuredCommand) => featuredCommand.Command == command || featuredCommand.Proxy == command);
        }
    }
}