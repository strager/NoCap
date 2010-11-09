using System;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using NoCap.Library.Tasks;

namespace NoCap.Library {
    public interface IPlugin : IDisposable, INamedComponent {
        UIElement GetEditor(IInfoStuff infoStuff);

        void Initialize(CommandRunner commandRunner, CompositionContainer compositionContainer);
    }
}
