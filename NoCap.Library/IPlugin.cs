using System;
using System.Windows;

namespace NoCap.Library {
    public interface IPlugin : IDisposable, INamedComponent {
        UIElement GetEditor(ICommandProvider commandProvider);

        void Initialize(IRuntimeProvider runtimeProvider);
    }
}
