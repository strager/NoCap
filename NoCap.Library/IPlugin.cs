using System;
using System.Windows;

namespace NoCap.Library {
    public interface IPlugin : IDisposable, INamedComponent {
        UIElement GetEditor(IInfoStuff infoStuff);

        void Initialize(IRuntimePluginInfo runtimePluginInfo);
    }
}
