using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using NoCap.Library;
using NoCap.Library.Tasks;
using NoCap.Library.Util;
using WinputDotNet;

namespace NoCap.GUI.WPF.Plugins {
    [Export(typeof(IPlugin)), Serializable]
    public class InputBindingsPlugin : IPlugin, ISerializable {
        [NonSerialized]
        private bool isSetUp = false;

        [NonSerialized]
        private bool isDisposed = false;

        [NonSerialized]
        private IEnumerable<IInputProvider> inputProviders;

        public string Name {
            get {
                return "Bindings";
            }
        }

        [NonSerialized]
        private CommandRunner commandRunner;

        public ObservableCollection<CommandBinding> Bindings {
            get;
            set;
        }

        public IInputProvider InputProvider {
            get;
            set;
        }

        public UIElement GetEditor(IInfoStuff infoStuff) {
            return new InputBindingsEditor(this, infoStuff);
        }

        public void Initialize(IRuntimePluginInfo runtimePluginInfo) {
            this.commandRunner = runtimePluginInfo.CommandRunner;

            var compositionContainer = runtimePluginInfo.CompositionContainer;

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

            var handle = IntPtr.Zero;
            
            InputProvider.CommandStateChanged += CommandStateChanged;
            InputProvider.SetBindings(Bindings);
            InputProvider.Attach(handle);
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
            InputProvider.SetBindings(Bindings);
        }

        protected InputBindingsPlugin(SerializationInfo info, StreamingContext context) {
            // TODO Use instance in inputProviders collection
            var inputProviderType = info.GetValue<Type>("InputProvider type");
            InputProvider = (IInputProvider) Activator.CreateInstance(inputProviderType);

            Bindings = info.GetValue<ObservableCollection<CommandBinding>>("Bindings");

            Bindings.CollectionChanged += (sender, e) => UpdateBindings();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("InputProvider type", InputProvider == null ? null : InputProvider.GetType());
            info.AddValue("Bindings", Bindings);
        }
    }
}
