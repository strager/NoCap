using System.Collections.Generic;
using System.Linq;
using NoCap.GUI.WPF.Settings;
using NoCap.Library;
using NoCap.Library.Extensions;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for DefaultCommandsEditor.xaml
    /// </summary>
    public partial class DefaultCommandsEditor {
        public ICommandProvider CommandProvider {
            get;
            set;
        }

        public IEnumerable<object> DefaultCommands {
            get;
            set;
        }

        public DefaultCommandsEditor(ICommandProvider commandProvider, IFeatureRegistry registry, FeaturedCommandCollection defaults) {
            CommandProvider = commandProvider;
            DefaultCommands = registry.RegisteredFeatures.Select((features) => new DefaultCommandItemThing(registry, features, defaults));

            InitializeComponent();
        }

        private void EnforceSelection(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            // HACK to prevent unselection
            if (this.featuresSelector.SelectedItems.Count != 0) {
                return;
            }

            var items = e.RemovedItems;

            if (items.Count == 0) {
                this.featuresSelector.SelectedIndex = 0;
            } else {
                this.featuresSelector.SelectedItem = items[0];
            }
        }
    }

    class DefaultCommandItemThing {
        private readonly IFeatureRegistry registry;
        private readonly CommandFeatures features;
        private readonly FeaturedCommandCollection defaults;

        public DefaultCommandItemThing(IFeatureRegistry registry, CommandFeatures features, FeaturedCommandCollection defaults) {
            this.registry = registry;
            this.features = features;
            this.defaults = defaults;
        }

        public CommandFeatures Features {
            get {
                return this.features;
            }
        }

        public string Name {
            get {
                return this.registry.GetFeaturesName(Features);
            }
        }

        public ICommand Command {
            get {
                return this.defaults[this.features];
            }

            set {
                this.defaults[this.features] = value;
            }
        }
    }
}
