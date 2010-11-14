﻿using System;
using System.Collections;
using System.Collections.Generic;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    public class FeaturedCommandCollection : ICollection<FeaturedCommand> {
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

        public bool Contains(FeaturedCommand item) {
            throw new NotSupportedException();
            return this.commands.ContainsKey(item.Features);
        }

        void ICollection<FeaturedCommand>.CopyTo(FeaturedCommand[] array, int arrayIndex) {
            this.commands.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(FeaturedCommand item) {
            return this.commands.Remove(item.Features);
        }

        public int Count {
            get {
                return this.commands.Count;
            }
        }

        bool ICollection<FeaturedCommand>.IsReadOnly {
            get {
                return false;
            }
        }
    }
}