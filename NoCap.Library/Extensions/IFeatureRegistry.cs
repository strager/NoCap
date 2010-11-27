using System.Collections.Generic;

namespace NoCap.Library.Extensions {
    public interface IFeatureRegistry {
        IEnumerable<CommandFeatures> RegisteredFeatures { get; }

        string GetFeaturesName(CommandFeatures features);

        void Register(CommandFeatures features, string name);
    }
}