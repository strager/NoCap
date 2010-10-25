using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using NoCap.GUI.WPF.Commands;
using NoCap.Library;
using WinputDotNet;
using INamedComponent = NoCap.Library.INamedComponent;

namespace NoCap.GUI.WPF {
    public class Providers {
        private readonly CompositionContainer compositionContainer;

        private Providers() :
            this(new CompositionContainer(
                new AggregateCatalog(
                    new DirectoryCatalog("."),
                    new AssemblyCatalog(typeof(Providers).Assembly)
                )
            )) {
        }

        private Providers(CompositionContainer compositionContainer) {
            this.compositionContainer = compositionContainer;

            this.compositionContainer.ComposeParts(this);
        }

        private static readonly Providers PrivateInstance = new Providers();

        public static Providers Instance {
            get {
                return PrivateInstance;
            }
        }

#pragma warning disable 649 // Field is never assigned
        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<IInputProvider> inputProviders;

        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<IProcessorFactory> processorFactories;
#pragma warning restore 649

        public IEnumerable<IInputProvider> InputProviders {
            get {
                return inputProviders;
            }
        }

        public IEnumerable<IProcessorFactory> ProcessorFactories {
            get {
                return this.processorFactories;
            }
        }
    }
}
