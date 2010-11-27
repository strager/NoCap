using System;
using System.ComponentModel.Composition.Hosting;
using NoCap.GUI.WPF.Settings;
using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Library.Tasks;

namespace NoCap.GUI.WPF {
    internal class ProgramPluginContext : IPluginContext {
        public ProgramPluginContext(CommandRunner commandRunner, ExtensionManager extensionManager, IFeatureRegistry featureRegistry) {
            this.commandRunner = commandRunner;
            this.extensionManager = extensionManager;
            this.featureRegistry = featureRegistry;
        }

        private readonly CommandRunner commandRunner;
        private readonly ExtensionManager extensionManager;
        private readonly IFeatureRegistry featureRegistry;

        public CommandRunner CommandRunner {
            get {
                return this.commandRunner;
            }
        }

        public CompositionContainer CompositionContainer {
            get {
                return ExtensionManager.CompositionContainer;
            }
        }

        public ExtensionManager ExtensionManager {
            get {
                return this.extensionManager;
            }
        }

        public IFeatureRegistry FeatureRegistry {
            get {
                return this.featureRegistry;
            }
        }
    }
}