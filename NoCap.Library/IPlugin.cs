using System;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using NoCap.Library.Tasks;

namespace NoCap.Library {
    public interface IPlugin : IDisposable, INamedComponent {
        CommandRunner CommandRunner { get; set; }

        void Populate(CompositionContainer compositionContainer);

        UIElement GetEditor(IInfoStuff infoStuff);

        void Init();
    }
}
