using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
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

#pragma warning disable 649 // Field is never assigned
        [ImportMany(AllowRecomposition = true), NonSerialized]
        private IEnumerable<IInputProvider> inputProviders;
#pragma warning restore 649

        [Import(AllowRecomposition = false)]
        private IInputProvider inputProvider;

        public string Name {
            get {
                return "Bindings";
            }
        }

        [NonSerialized]
        private CommandRunner commandRunner;

        CommandRunner IPlugin.CommandRunner {
            get {
                return this.commandRunner;
            }

            set {
                this.commandRunner = value;
            }
        }

        public ObservableCollection<CommandBinding> Bindings {
            get;
            set;
        }

        public IInputProvider InputProvider {
            get {
                return this.inputProvider;
            }

            set {
                this.inputProvider = value;
            }
        }

        public void Populate(CompositionContainer compositionContainer) {
            compositionContainer.ComposeParts(this);
        }

        public UIElement GetEditor(IInfoStuff infoStuff) {
            return new InputBindingsEditor(this, infoStuff);
        }

        public void Init() {
            SetUp();
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

                if (this.commandRunner != null) {
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

        protected InputBindingsPlugin(SerializationInfo info, StreamingContext context) {
            // TODO Use instance in inputProviders collection
            var inputProviderType = info.GetValue<Type>("InputProvider type");
            this.inputProvider = (IInputProvider) Activator.CreateInstance(inputProviderType);

            Bindings = info.GetValue<ObservableCollection<CommandBinding>>("Bindings");

            Bindings.CollectionChanged += (sender, e) => UpdateBindings();
        }

        private void UpdateBindings() {
            this.inputProvider.SetBindings(Bindings);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("InputProvider type", InputProvider.GetType());
            info.AddValue("Bindings", Bindings);
        }
    }
}
