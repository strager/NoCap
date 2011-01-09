using System.ComponentModel.Composition.Hosting;
using NoCap.Library.Tasks;

namespace NoCap.Library.Extensions {
    public interface IPluginContext {
        ICommandRunner CommandRunner { get; }

        IFeatureRegistry FeatureRegistry { get; }
        ICommandProvider CommandProvider { get; }
    }
}