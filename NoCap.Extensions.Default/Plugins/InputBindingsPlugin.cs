using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Library.Tasks;
using NoCap.Library.Util;
using WinputDotNet;

namespace NoCap.Extensions.Default.Plugins {
    [Export(typeof(IPlugin))]
    [DataContract(Name = "InputBindingsPlugin")]
    public class InputBindingsPlugin : IPlugin, IExtensibleDataObject {
        private bool isSetUp = false;
        private bool isDisposed = false;

        private IEnumerable<IInputProvider> inputProviders;

        public string Name {
            get {
                return "Bindings";
            }
        }

        private ICommandRunner commandRunner;

        [DataMember(Name = "CommandBindings")]
        public ObservableCollection<CommandBinding> Bindings {
            get;
            set;
        }

        public IEnumerable<IInputProvider> InputProviders {
            get {
                return this.inputProviders;
            }
        }

        public IInputProvider InputProvider {
            get;
            set;
        }

        [DataMember(Name = "InputProviderType")]
        private Type InputProviderType {
            get {
                var inputProvider = InputProvider;

                return inputProvider == null ? null : inputProvider.GetType();
            }

            set {
                // TODO Use instance in inputProviders collection
                InputProvider = (IInputProvider) Activator.CreateInstance(value);
            }
        }

        public UIElement GetEditor(ICommandProvider commandProvider) {
            return new InputBindingsEditor(this, commandProvider);
        }

        public void Initialize(IPluginContext pluginContext) {
            this.commandRunner = pluginContext.CommandRunner;

            var compositionContainer = pluginContext.CompositionContainer;

            Recompose(compositionContainer);
            compositionContainer.ExportsChanged += (sender, e) => Recompose(compositionContainer);

            InputProvider = this.inputProviders.FirstOrDefault();

            SetUp();
        }

        private void Recompose(CompositionContainer compositionContainer) {
            this.inputProviders = compositionContainer.GetExportedValues<IInputProvider>();
        }

        internal void SetUp() {
            EnsureNotDisposed();

            if (this.isSetUp) {
                throw new InvalidOperationException("Already set up");
            }

            SetUpInput();

            this.isSetUp = true;
        }

        internal void ShutDown() {
            EnsureNotDisposed();

            if (!this.isSetUp) {
                throw new InvalidOperationException("Not set up");
            }

            ShutDownInput();

            this.isSetUp = false;
        }

        private void SetUpInput() {
            if (InputProvider == null) {
                return;
            }
            
            InputProvider.CommandStateChanged += CommandStateChanged;
            UpdateBindings();
            InputProvider.Attach();
        }

        private void ShutDownInput() {
            if (InputProvider == null) {
                return;
            }

            InputProvider.Detach();
            InputProvider.CommandStateChanged -= CommandStateChanged;
        }

        private void CommandStateChanged(object sender, CommandStateChangedEventArgs e) {
            if (e.State == InputState.On) {
                var command = (BoundCommand) e.Command;

                if (this.commandRunner != null && command.Command.IsValidAndNotNull()) {
                    this.commandRunner.Run(command.Command);
                }
            }
        }

        public void Dispose() {
            EnsureNotDisposed();

            if (this.isSetUp) {
                ShutDown();
            }

            this.isDisposed = true;
        }

        private void EnsureNotDisposed() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("InputBindingsPlugin");
            }
        }

        public InputBindingsPlugin() {
            Bindings = new ObservableCollection<CommandBinding>();

            Bindings.CollectionChanged += (sender, e) => UpdateBindings();
        }

        private void UpdateBindings() {
            InputProvider.SetBindings(Bindings.Where((binding) => binding.Input != null));
        }

        protected InputBindingsPlugin(SerializationInfo info, StreamingContext context) {
            // TODO Use instance in inputProviders collection
            var inputProviderType = info.GetValue<Type>("InputProvider type");
            InputProvider = (IInputProvider) Activator.CreateInstance(inputProviderType);

            Bindings = info.GetValue<ObservableCollection<CommandBinding>>("Bindings");
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            if (Bindings == null) {
                Bindings = new ObservableCollection<CommandBinding>();
            }

            Bindings.CollectionChanged += (sender, e) => UpdateBindings();
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}
