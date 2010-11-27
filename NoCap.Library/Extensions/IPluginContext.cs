using System.ComponentModel.Composition.Hosting;
using NoCap.Library.Tasks;

namespace NoCap.Library.Extensions {
    public interface IPluginContext {
        CommandRunner CommandRunner { get; }
        CompositionContainer CompositionContainer { get; }
        IFeatureRegistry FeatureRegistry { get; }
    }
}