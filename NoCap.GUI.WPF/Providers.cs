using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using WinputDotNet;

namespace NoCap.GUI.WPF {
    public class Providers {
        private static readonly CompositionContainer CompositionContainer = new CompositionContainer(new DirectoryCatalog("."));

        private Providers() {
			CompositionContainer.ComposeParts(this);
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
#pragma warning restore 649

        public IEnumerable<IInputProvider> InputProviders {
            get {
                return inputProviders;
            }
        }
    }
}
