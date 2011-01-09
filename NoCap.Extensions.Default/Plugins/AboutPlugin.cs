using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Windows;
using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Update;

namespace NoCap.Extensions.Default.Plugins {
    [Export(typeof(IPlugin))]
    [DataContract(Name = "AboutPlugin")]
    sealed class AboutPlugin : IPlugin, IExtensibleDataObject {
        public void Dispose() {
            // Do nothing.
        }

        public string Name {
            get {
                return "About";
            }
        }

        public UIElement GetEditor(ICommandProvider commandProvider) {
            return new AboutEditor {
                DataContext = new {
                    PatchingEnvironment = PatchingEnvironment.GetCurrent(),
                }
            };
        }

        public void Initialize(IPluginContext pluginContext) {
            // Do nothing.
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}
