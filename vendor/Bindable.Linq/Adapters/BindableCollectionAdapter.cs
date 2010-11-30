namespace Bindable.Linq.Adapters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Configuration;
    using Helpers;

    /// <summary>
    /// Turns any kind of collection into a bindable collection.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    internal sealed class BindableCollectionAdapter<TElement> : IBindableCollection<TElement>, IConfigurable
        where TElement : class
    {
        private readonly EventHandler<NotifyCollectionChangedEventArgs> _collectionChangedHandler;
        private readonly WeakEventReference<NotifyCollectionChangedEventArgs> _collectionWeakHandler;
        private readonly IBindingConfiguration _configuration;
        private readonly EventHandler<PropertyChangedEventArgs> _propertyChangedHander;
        private readonly WeakEventReference<PropertyChangedEventArgs> _propertyWeakHandler;
        private readonly IEnumerable _source;
        private readonly StateScope _suspendCollectionChangedState = new StateScope();
        private readonly bool _throwOnInvalidCast;
#if !SILVERLIGHT
        // Silverlight does not support ListChanged events.
        private readonly EventHandler<ListChangedEventArgs> _listChangedHandler;
        private readonly WeakEventReference<ListChangedEventArgs> _listWeakHandler;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="BindableCollectionAdapter&lt;TElement&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="throwOnInvalidCast">Indicates whether InvalidCastExceptions will be thrown if a source element cannot be converted to the target type.</param>
        /// <param name="bindingConfiguration">The binding configuration.</param>
        public BindableCollectionAdapter(IEnumerable source, bool throwOnInvalidCast, IBindingConfiguration bindingConfiguration)
        {
            source.ShouldNotBeNull("source");
            _source = source;
            _throwOnInvalidCast = throwOnInvalidCast;
            _configuration = bindingConfiguration;

            var sourceAsPropertyChanged = source as INotifyPropertyChanged;
            var sourceAsCollectionChanged = source as INotifyCollectionChanged;
#if !SILVERLIGHT
            // Silverlight does not provide an IBindingList interface
            var sourceAsBindingList = source as IBindingList;
#endif
            if (sourceAsPropertyChanged != null)
            {
                _propertyChangedHander = Source_PropertyChanged;
                _propertyWeakHandler = new WeakEventReference<PropertyChangedEventArgs>(_propertyChangedHander);
                sourceAsPropertyChanged.PropertyChanged += _propertyWeakHandler.WeakEventHandler;
            }
            if (sourceAsCollectionChanged != null)
            {
                _collectionChangedHandler = Source_CollectionChanged;
                _collectionWeakHandler = new WeakEventReference<NotifyCollectionChangedEventArgs>(_collectionChangedHandler);
                sourceAsCollectionChanged.CollectionChanged += _collectionWeakHandler.WeakEventHandler;
            }
#if !SILVERLIGHT
                // Silverlight does not provide an IBindingList interface
            else if (sourceAsBindingList != null)
            {
                _listChangedHandler = Source_ListChanged;
                _listWeakHandler = new WeakEventReference<ListChangedEventArgs>(_listChangedHandler);
                sourceAsBindingList.ListChanged += _listWeakHandler.WeakEventHandler;
            }
#endif
        }

        #region IBindableCollection<TElement> Members
        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the count of items in the collection.
        /// </summary>
        /// <value></value>
        public int Count
        {
            get
            {
                var result = 0;
                foreach (object element in this)
                {
                    result++;
                }
                return result;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            using (_suspendCollectionChangedState.Enter())
            {
                foreach (var element in _source)
                {
                    if (element != null)
                    {
                        if (element is TElement)
                        {
                            yield return (TElement) element;
                        }
                        else if (_throwOnInvalidCast)
                        {
                            throw new InvalidCastException("Could not cast object of type {0} to type {1}".FormatWith(element.GetType(), typeof (TElement)));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_propertyWeakHandler != null)
            {
                _propertyWeakHandler.Dispose();
            }
            if (_collectionWeakHandler != null)
            {
                _collectionWeakHandler.Dispose();
            }
#if !SILVERLIGHT
            // Silverlight does not provide ListChanged events
            if (_listWeakHandler != null)
            {
                _listWeakHandler.Dispose();
            }
#endif
        }
        #endregion

        #region IConfigurable Members
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IBindingConfiguration Configuration
        {
            get { return _configuration; }
        }
        #endregion

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

#if !SILVERLIGHT
        // Silverlight does not provide ListChanged events
        private void Source_ListChanged(object sender, ListChangedEventArgs e)
        {
            var list = sender as IBindingList;
            if (list != null)
            {
                NotifyCollectionChangedEventArgs argumentsToRaise = null;
                switch (e.ListChangedType)
                {
                    case ListChangedType.ItemAdded:
                    {
                        var array = new object[list.Count];
                        list.CopyTo(array, 0);
                        if (e.NewIndex >= 0 && e.NewIndex < array.Length)
                        {
                            var itemAtIndex = list[e.NewIndex];
                            argumentsToRaise = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemAtIndex, e.NewIndex);
                        }
                    }
                        break;
                    case ListChangedType.ItemDeleted:
                    {
                        var array = new object[list.Count];
                        list.CopyTo(array, 0);
                        if (e.OldIndex >= 0 && e.OldIndex < array.Length)
                        {
                            var itemAtIndex = list[e.OldIndex];
                            argumentsToRaise = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemAtIndex, e.NewIndex);
                        }
                    }
                        break;
                    case ListChangedType.ItemMoved:
                    {
                        var array = new object[list.Count];
                        list.CopyTo(array, 0);
                        if (e.OldIndex >= 0 && e.OldIndex < array.Length)
                        {
                            var itemAtIndex = list[e.NewIndex];
                            argumentsToRaise = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, itemAtIndex, e.NewIndex, e.OldIndex);
                        }
                    }
                        break;
                    case ListChangedType.ItemChanged:
                    case ListChangedType.Reset:
                        argumentsToRaise = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                        break;
                }

                OnCollectionChanged(argumentsToRaise);
            }
        }
#endif

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_suspendCollectionChangedState.IsWithin)
            {
                return;
            }

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suspendCollectionChangedState.IsWithin)
            {
                var handler = CollectionChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }
    }
}