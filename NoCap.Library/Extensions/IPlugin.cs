using System;
using System.Windows;

namespace NoCap.Library.Extensions {
    public interface IPlugin : IDisposable, INamedComponent {
        UIElement GetEditor(ICommandProvider commandProvider);

        void Initialize(IPluginContext pluginContext);
    }
}
