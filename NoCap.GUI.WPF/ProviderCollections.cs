using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using WinputDotNet;

namespace NoCap.GUI.WPF {
    public class ProviderCollections {
        private static readonly CompositionContainer CompositionContainer = new CompositionContainer(new DirectoryCatalog("."));

        public ProviderCollections() {
			CompositionContainer.ComposeParts(this);
        }

#pragma warning disable 649 // Field is never assigned
        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<IInputProvider> inputProviders;
#pragma warning restore 649

        public IEnumerable<IInputProvider> InputProviders {
            get {
                return inputProviders;
            }
        }

        public ProviderModules GetDefaultModules() {
            return new ProviderModules {
                InputProvider = InputProviders.FirstOrDefault()
            };
        }
    }
}
