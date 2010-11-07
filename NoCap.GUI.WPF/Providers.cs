using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using NoCap.Library;

namespace NoCap.GUI.WPF {
    public class Providers {
        public static readonly CompositionContainer CompositionContainer = new CompositionContainer(
            new AggregateCatalog(
                new DirectoryCatalog("."),
                new AssemblyCatalog(typeof(Providers).Assembly)
            )
        );

        private Providers(CompositionContainer compositionContainer) {
            compositionContainer.ComposeParts(this);
        }

        private static readonly Providers PrivateInstance = new Providers(CompositionContainer);

        public static Providers Instance {
            get {
                return PrivateInstance;
            }
        }

#pragma warning disable 649 // Field is never assigned
        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<ICommandFactory> commandFactories;
#pragma warning restore 649

        public IEnumerable<ICommandFactory> CommandFactories {
            get {
                return this.commandFactories;
            }
        }
    }
}
