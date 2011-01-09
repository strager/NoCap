using System;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Windows;
using NoCap.Library;
using NoCap.Library.Extensions;

namespace NoCap.Extensions.Default.Plugins {
    [Export(typeof(IPlugin))]
    [DataContract(Name = "CommandEditorPlugin")]
    class CommandEditorPlugin : IPlugin, IExtensibleDataObject {
        public string Name {
            get {
                return "Commands";
            }
        }

        public UIElement GetEditor(ICommandProvider commandProvider) {
            return new CommandSettingsEditor();
        }

        void IPlugin.Initialize(IPluginContext pluginContext) {
            // Do nothing.
        }

        void IDisposable.Dispose() {
            // Do nothing.
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}
