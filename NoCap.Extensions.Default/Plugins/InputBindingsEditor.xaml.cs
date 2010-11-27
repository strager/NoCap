using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using NoCap.Library;
using WinputDotNet;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.Extensions.Default.Plugins {
    /// <summary>
    /// Interaction logic for InputBindingsEditor.xaml
    /// </summary>
    public partial class InputBindingsEditor {
        private readonly InputBindingsPlugin plugin;
        private readonly ICommandProvider commandProvider;

        private MutableCommandBinding SelectedBinding {
            get {
                return (MutableCommandBinding) this.bindingsList.SelectedItem;
            }
        }

        public MutableCommandBindingCollection Bindings {
            get;
            set;
        }

        public IEnumerable<IInputProvider> InputProviders {
            get {
                return this.plugin.InputProviders;
            }
        }

        public IInputProvider InputProvider {
            get {
                return this.plugin.InputProvider;
            }

            set {
                this.plugin.InputProvider = value;
            }
        }

        public InputBindingsPlugin Plugin {
            get {
                return this.plugin;
            }
        }

        public ICommandProvider CommandProvider {
            get {
                return this.commandProvider;
            }
        }

        public InputBindingsEditor(InputBindingsPlugin plugin, ICommandProvider commandProvider) {
            this.plugin = plugin;
            this.commandProvider = commandProvider;

            InitializeComponent();

            Bindings = new MutableCommandBindingCollection(plugin.Bindings);

            DataContext = this;

            CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.New, AddBinding, CanAddBinding));
            CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Open, EditBinding, CanEditBinding));
            CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Delete, DeleteBinding, CanDeleteBinding));
        }

        private void CanAddBinding(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void CanEditBinding(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (SelectedBinding != null);
            e.Handled = true;
        }

        private void CanDeleteBinding(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (SelectedBinding != null);
            e.Handled = true;
        }

        private void AddBinding(object sender, RoutedEventArgs e) {
            IInputSequence inputSequence;

            if (!TryGetInputSequence(out inputSequence)) {
                return;
            }

            var binding = new MutableCommandBinding(inputSequence, null);

            // If a binding already exists, remove it and re-add it as a new instance
            // to push it at the end of the list.
            var existingBinding = Bindings.FirstOrDefault((b) => b.Input.Equals(inputSequence));

            if (existingBinding != null) {
                Bindings.Remove(existingBinding);
                binding.Command = existingBinding.Command;
            }

            Bindings.Add(binding);
        }

        private void EditBinding(object sender, RoutedEventArgs e) {
            if (SelectedBinding != null) {
                EditBinding(SelectedBinding);
            }
        }

        private void DeleteBinding(object sender, RoutedEventArgs e) {
            if (SelectedBinding != null) {
                Bindings.Remove(SelectedBinding);
            }
        }

        private void EditBinding(MutableCommandBinding binding) {
            if (binding == null) {
                throw new ArgumentNullException("binding");
            }

            IInputSequence inputSequence;

            if (!TryGetInputSequence(out inputSequence)) {
                return;
            }

            binding.Input = inputSequence;
        }

        private bool TryGetInputSequence(out IInputSequence inputSequence) {
            Plugin.ShutDown();

            var window = new BindWindow(Plugin.InputProvider);
            bool success = window.ShowDialog() == true;

            inputSequence = success ? window.InputSequence : null;

            Plugin.SetUp();

            return success;
        }
    }

    class MutableCommandBindingMapping {
        private readonly MutableCommandBinding mutableCommandBinding;
        private readonly CommandBinding immutableCommandBinding;

        public MutableCommandBindingMapping(MutableCommandBinding mutableCommandBinding, CommandBinding immutableCommandBinding) {
            this.mutableCommandBinding = mutableCommandBinding;
            this.immutableCommandBinding = immutableCommandBinding;
        }

        public MutableCommandBinding MutableCommandBinding {
            get {
                return this.mutableCommandBinding;
            }
        }

        public CommandBinding ImmutableCommandBinding {
            get {
                return this.immutableCommandBinding;
            }
        }
    }

    public class MutableCommandBindingCollection : ICollection<MutableCommandBinding>, INotifyCollectionChanged {
        private readonly ICollection<CommandBinding> originalBindings;
        private readonly ICollection<MutableCommandBindingMapping> mappings = new List<MutableCommandBindingMapping>();

        public MutableCommandBindingCollection(ICollection<CommandBinding> originalBindings) {
            this.originalBindings = originalBindings;

            foreach (var binding in this.originalBindings) {
                var mutableBinding = GetMutableBinding(binding);
                mutableBinding.PropertyChanged += UpdateBindingHandler;

                this.mappings.Add(new MutableCommandBindingMapping(mutableBinding, binding));
            }
        }

        private MutableCommandBinding GetMutableBinding(CommandBinding binding) {
            var mapping = this.mappings.FirstOrDefault((m) => ReferenceEquals(m.ImmutableCommandBinding, binding));

            if (mapping == null) {
                return new MutableCommandBinding(binding);
            }

            return mapping.MutableCommandBinding;
        }

        private bool RemoveMutableBinding(MutableCommandBinding binding) {
            var mapping = this.mappings.FirstOrDefault((m) => ReferenceEquals(m.MutableCommandBinding, binding));
            this.mappings.Remove(mapping);

            var immutableBinding = mapping.ImmutableCommandBinding;
            this.originalBindings.Remove(immutableBinding);

            return true;
        }

        private CommandBinding GetImmutableBinding(MutableCommandBinding binding) {
            var mapping = this.mappings.FirstOrDefault((m) => ReferenceEquals(m.MutableCommandBinding, binding));

            if (mapping == null) {
                return new CommandBinding(binding.Input, binding.Command);
            }

            return mapping.ImmutableCommandBinding;
        }

        private void UpdateBindingHandler(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            UpdateBinding((MutableCommandBinding) sender);
        }

        private void UpdateBinding(MutableCommandBinding binding) {
            // Be careful with the order here.

            Remove(this.mappings.First((m) => ReferenceEquals(m.MutableCommandBinding, binding)));

            Add(binding, GetImmutableBinding(binding));
        }

        private void Remove(MutableCommandBindingMapping mapping) {
            this.originalBindings.Remove(mapping.ImmutableCommandBinding);
            this.mappings.Remove(mapping);
        }

        public IEnumerator<MutableCommandBinding> GetEnumerator() {
            return this.mappings.Select((m) => m.MutableCommandBinding).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(MutableCommandBinding item) {
            if (Contains(item)) {
                throw new InvalidOperationException("Cannot add same item twice");
            }

            item.PropertyChanged += UpdateBindingHandler;

            var immutableBinding = GetImmutableBinding(item);

            Add(item, immutableBinding);

            Notify(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        private void Add(MutableCommandBinding mutableCommandBinding, CommandBinding immutableCommandBinding) {
            this.mappings.Add(new MutableCommandBindingMapping(mutableCommandBinding, immutableCommandBinding));
            this.originalBindings.Add(immutableCommandBinding);
        }

        public void Clear() {
            this.mappings.Clear();
            this.originalBindings.Clear();

            Notify(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(MutableCommandBinding item) {
            return this.mappings.Any((m) => ReferenceEquals(m.MutableCommandBinding, item));
        }

        public void CopyTo(MutableCommandBinding[] array, int arrayIndex) {
            // Lazy/inefficient way.
            new List<MutableCommandBinding>(this).CopyTo(array, arrayIndex);
        }

        public bool Remove(MutableCommandBinding item) {
            var mapping = this.mappings.FirstOrDefault((m) => ReferenceEquals(m.MutableCommandBinding, item));

            if (mapping == null) {
                return false;
            }

            int itemIndex = this.mappings.TakeWhile((m) => m != mapping).Count();

            if (!RemoveMutableBinding(item)) {
                return false;
            }

            item.PropertyChanged -= UpdateBindingHandler;

            this.originalBindings.Remove(mapping.ImmutableCommandBinding);

            Notify(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, itemIndex));

            return true;
        }

        public int Count {
            get {
                return this.mappings.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Notify(NotifyCollectionChangedEventArgs e) {
            var handler = CollectionChanged;

            if (handler != null) {
                handler(this, e);
            }
        }
    }

    public class MutableCommandBinding : INotifyPropertyChanged {
        private IInputSequence input;
        private ICommand command;

        public IInputSequence Input {
            get {
                return this.input;
            }

            set {
                this.input = value;

                Notify("Input");
            }
        }

        public ICommand Command {
            get {
                return this.command;
            }

            set {
                this.command = value;

                Notify("Command");
            }
        }

        public MutableCommandBinding(IInputSequence input, ICommand command) {
            this.input = input;
            this.command = command;
        }

        public MutableCommandBinding(CommandBinding source) :
            this(source.Input, source.Command) {
        }

        private void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class NullableBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
