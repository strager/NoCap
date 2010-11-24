using System.ComponentModel.Composition.Hosting;
using NoCap.Library.Tasks;

namespace NoCap.Library {
    public interface IRuntimePluginInfo {
        CommandRunner CommandRunner { get; }
        CompositionContainer CompositionContainer { get; }

        void RegisterDefaultType(CommandFeatures features, string name);
    }
}