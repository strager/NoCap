using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using NoCap.Library.Tasks;

namespace NoCap.Library {
    public interface IRuntimeProvider {
        CommandRunner CommandRunner { get; }
        CompositionContainer CompositionContainer { get; }
        IFeatureRegistry FeatureRegistry { get; }
    }

    public interface IFeatureRegistry {
        IEnumerable<CommandFeatures> RegisteredFeatures { get; }

        string GetFeaturesName(CommandFeatures features);

        void Register(CommandFeatures features, string name);
    }
}