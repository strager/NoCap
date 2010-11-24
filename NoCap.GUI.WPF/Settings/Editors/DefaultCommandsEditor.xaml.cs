using System.Collections.Generic;
using System.Linq;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings.Editors {
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
