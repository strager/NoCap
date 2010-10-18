using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using NoCap.GUI.WPF.Templates;
using NoCap.Library;
using WinputDotNet;

namespace NoCap.GUI.WPF {
    public class Providers {
        private readonly CompositionContainer compositionContainer;

        private Providers() :
            this(new CompositionContainer(new DirectoryCatalog("."))) {
        }

        private Providers(CompositionContainer compositionContainer) {
            this.compositionContainer = compositionContainer;

            compositionContainer.ComposeParts(this);
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
        private IEnumerable<ISource> sources;

        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<IDestination> destinations;

        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<ICommandFactory> templateFactories;
#pragma warning restore 649

        public IEnumerable<IInputProvider> InputProviders {
            get {
                return inputProviders;
            }
        }

        public IEnumerable<ISource> Sources {
            get {
                return this.sources;
            }
        }

        public IEnumerable<IDestination> Destinations {
            get {
                return this.destinations;
            }
        }

        public IEnumerable<ICommandFactory> TemplateFactories {
            get {
                return this.templateFactories;
            }
        }
    }
}
