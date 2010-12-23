using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Library.Tasks;

namespace NoCap.GUI.WPF.Runtime {
    internal class ProgramPluginContext : IPluginContext {
        public ProgramPluginContext(CommandRunner commandRunner, IFeatureRegistry featureRegistry, ICommandProvider commandProvider) {
            this.commandRunner = commandRunner;
            this.featureRegistry = featureRegistry;
            this.commandProvider = commandProvider;
        }

        private readonly CommandRunner commandRunner;
        private readonly IFeatureRegistry featureRegistry;
        private readonly ICommandProvider commandProvider;

        public ICommandRunner CommandRunner {
            get {
                return this.commandRunner;
            }
        }

        public IFeatureRegistry FeatureRegistry {
            get {
                return this.featureRegistry;
            }
        }

        public ICommandProvider CommandProvider {
            get {
                return this.commandProvider;
            }
        }
    }
}