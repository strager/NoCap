using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using Bindable.Linq;
using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Library.Tasks;
using WinputDotNet;
using ICommand = NoCap.Library.ICommand;

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

        private IInputProvider inputProvider;

        public IInputProvider InputProvider {
            get {
                return this.inputProvider;
            }

            set {
                this.inputProvider = value;

                this.inputProviderTypeName = value == null ? null : value.GetType().AssemblyQualifiedName;
            }
        }

        private IEnumerable<CommandBinding> GetDefaultBindings(IInputProvider inputProvider, ICommand command) {
            if (command == null) {
                yield break;
            }

            var attributes = command.GetType().GetCustomAttributes(typeof(DefaultBindingAttribute), false).OfType<DefaultBindingAttribute>();

            foreach (var defaultBinding in attributes.Where((attr) => attr.ProviderType == inputProvider.GetType())) {
                yield return new CommandBinding(defaultBinding.InputSequence, command);
            }
        }

        [DataMember(Name = "InputProviderTypeName")]
        private string inputProviderTypeName;

        private IBindableCollection<ICommand> standAloneCommands;

        public UIElement GetEditor(ICommandProvider commandProvider) {
            return new InputBindingsEditor(this, commandProvider);
        }

        public void Initialize(IPluginContext pluginContext) {
            this.commandRunner = pluginContext.CommandRunner;

            this.standAloneCommands = pluginContext.CommandProvider.StandAloneCommands;

            this.standAloneCommands.CollectionChanged += (sender, e) => {
                foreach (var command in e.NewItems.OfType<ICommand>()) {
                    IncludeDefaultBindings(command);
                }
            };

            string extensionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var compositionContainer = new CompositionContainer(new DirectoryCatalog(extensionPath));

            Recompose(compositionContainer);
            compositionContainer.ExportsChanged += (sender, e) => Recompose(compositionContainer);

            SetUp();
        }

        private void IncludeDefaultBindings(ICommand command) {
            if (Bindings.Any((binding) => binding.Command == command)) {
                return;
            }

            foreach (var defaultBinding in GetDefaultBindings(InputProvider, command)) {
                Bindings.Add(defaultBinding);
            }
        }

        private void Recompose(CompositionContainer compositionContainer) {
            // Take care when touching this.

            this.inputProviders = compositionContainer.GetExportedValues<IInputProvider>();

            var defaultProvider = this.inputProviders.FirstOrDefault();
            var newProvider = InputProvider ?? defaultProvider;

            if (this.inputProviderTypeName != null && InputProvider == null) {
                newProvider = this.inputProviders.FirstOrDefault(
                    (provider) => provider.GetType().AssemblyQualifiedName == this.inputProviderTypeName
                ) ?? newProvider;
            }

            InputProvider = newProvider;

            foreach (var command in this.standAloneCommands) {
                IncludeDefaultBindings(command);
            }
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

        private void UpdateBindings() {
            if (InputProvider != null) {
                InputProvider.SetBindings(Bindings.Where((binding) => binding.Input != null));
            }
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
