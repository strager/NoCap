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

namespace NoCap.GUI.WPF.Plugins {
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

    public class MutableCommandBindingCollection : ICollection<MutableCommandBinding>, INotifyCollectionChanged {
        private readonly ICollection<CommandBinding> originalBindings;
        private readonly IDictionary<MutableCommandBinding, CommandBinding> mapping = new Dictionary<MutableCommandBinding, CommandBinding>();

        public MutableCommandBindingCollection(ICollection<CommandBinding> originalBindings) {
            this.originalBindings = originalBindings;

            foreach (var binding in this.originalBindings) {
                var mutableBinding = GetMutableBinding(binding);
                mutableBinding.PropertyChanged += UpdateBindingHandler;

                mapping[mutableBinding] = binding;
            }
        }

        private MutableCommandBinding GetMutableBinding(CommandBinding binding) {
            var mutableBinding = this.mapping.FirstOrDefault((kvp) => ReferenceEquals(kvp.Value, binding)).Key;

            if (mutableBinding == null) {
                mutableBinding = new MutableCommandBinding(binding);
            }

            return mutableBinding;
        }

        private bool RemoveMutableBinding(MutableCommandBinding binding) {
            var immutableBinding = this.mapping[binding];

            this.mapping.Remove(binding);
            this.originalBindings.Remove(immutableBinding);

            return true;
        }

        private CommandBinding GetImmutableBinding(MutableCommandBinding binding) {
            CommandBinding immutableBinding;

            if (!this.mapping.TryGetValue(binding, out immutableBinding)) {
                immutableBinding = new CommandBinding(binding.Input, binding.Command);
            }

            return immutableBinding;
        }

        private void UpdateBindingHandler(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            UpdateBinding((MutableCommandBinding) sender);
        }

        private void UpdateBinding(MutableCommandBinding binding) {
            // Be careful with the order here.

            var originalBinding = this.mapping[binding];
            this.originalBindings.Remove(originalBinding);
            this.mapping.Remove(binding);

            var newBinding = GetImmutableBinding(binding);
            this.mapping[binding] = newBinding;
            this.originalBindings.Add(newBinding);
        }

        public IEnumerator<MutableCommandBinding> GetEnumerator() {
            return this.mapping.Keys.GetEnumerator();
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

            this.mapping[item] = immutableBinding;
            this.originalBindings.Add(immutableBinding);

            Notify(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear() {
            this.mapping.Clear();
            this.originalBindings.Clear();

            Notify(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(MutableCommandBinding item) {
            return this.mapping.ContainsKey(item);
        }

        public void CopyTo(MutableCommandBinding[] array, int arrayIndex) {
            this.mapping.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(MutableCommandBinding item) {
            CommandBinding immutableBinding;
            bool itemRemoved = false;

            if (this.mapping.TryGetValue(item, out immutableBinding)) {
                int itemIndex = this.mapping.TakeWhile((kvp) => kvp.Key != item).Count();

                itemRemoved = RemoveMutableBinding(item);

                if (itemRemoved) {
                    item.PropertyChanged -= UpdateBindingHandler;

                    this.originalBindings.Remove(immutableBinding);

                    Notify(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, itemIndex));
                }
            }

            return itemRemoved;
        }

        public int Count {
            get {
                return this.mapping.Count;
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
