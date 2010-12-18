using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using NoCap.Extensions.Default.Helpers;
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

            foreach (var command in commandProvider.StandAloneCommands) {
                if (!Bindings.Any((binding) => ReferenceEquals(binding.Command, command))) {
                    Bindings.Add(new MutableCommandBinding(null, command));
                }
            }

            DataContext = this;

            CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Open, EditBinding, CanEditBinding));
            CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Delete, UnsetBinding, CanUnsetBinding));
        }

        private void ResizeColumnsHack() {
            // HACK Clearly a hack, taken from http://stackoverflow.com/questions/845269/force-resize-of-gridview-columns-inside-listview/1007643#1007643
            // We use BeginInvoke to allow binding to occur to hide/show
            // "(Remove)" and stuff which affects the column size.

            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                var gridView = bindingsList.View as GridView;

                if (gridView == null) {
                    return;
                }

                foreach (var column in gridView.Columns) {
                    // Code below was found in GridViewColumnHeader.OnGripperDoubleClicked() event handler (using Reflector)
                    // i.e. it is the same code that is executed when the gripper is double clicked
                    if (double.IsNaN(column.Width)) {
                        column.Width = column.ActualWidth;
                    }

                    column.Width = double.NaN; // Auto
                }
            }));
        }

        private void CanEditBinding(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (SelectedBinding != null) || (e.Parameter is MutableCommandBinding);
            e.Handled = true;
        }

        private void CanUnsetBinding(object sender, CanExecuteRoutedEventArgs e) {
            var binding = e.Parameter as MutableCommandBinding ?? SelectedBinding;

            e.CanExecute = binding != null && binding.Input != null;
            e.Handled = true;
        }

        private void EditBinding(object sender, ExecutedRoutedEventArgs e) {
            var binding = e.Parameter as MutableCommandBinding ?? SelectedBinding;

            if (binding != null) {
                EditBinding(binding);
            }
        }

        private void EditBinding(MutableCommandBinding binding) {
            if (binding == null) {
                throw new ArgumentNullException("binding");
            }

            IInputSequence inputSequence;

            if (!TryGetInputSequence(out inputSequence, binding.Command)) {
                return;
            }

            binding.Input = inputSequence;

            ResizeColumnsHack();
        }

        private void UnsetBinding(object sender, ExecutedRoutedEventArgs e) {
            var binding = e.Parameter as MutableCommandBinding ?? SelectedBinding;

            if (binding != null) {
                UnsetBinding(binding);
            }
        }

        private void UnsetBinding(MutableCommandBinding binding) {
            if (binding == null) {
                throw new ArgumentNullException("binding");
            }

            binding.Input = null;

            ResizeColumnsHack();
        }

        private bool TryGetInputSequence(out IInputSequence inputSequence, ICommand command) {
            Plugin.ShutDown();

            var window = new BindWindow(Plugin.InputProvider) {
                DataContext = new {
                    Command = command,
                }
            };

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

    public class InverseBooleanToVisibilityConverter : IValueConverter {
        private readonly BooleanToVisibilityConverter sourceConverter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool) {
                value = !((bool) value);
            }

            return this.sourceConverter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var newValue = this.sourceConverter.ConvertBack(value, targetType, parameter, culture);

            if (newValue is bool) {
                newValue = !((bool) value);
            }

            return newValue;
        }
    }
}
