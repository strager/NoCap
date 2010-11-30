namespace Bindable.Linq.Iterators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using Collections;
    using Configuration;
    using Dependencies;
    using Helpers;

    /// <summary>
    /// Serves as a base class for all Bindable LINQ Iterator containers. Iterators are Bindable LINQ operations 
    /// which take one or more collections of items, and return a collection of items. This is in contrast
    /// with <see cref="T:Aggregator`2">Aggregators</see>, which take one or more collections but return 
    /// a single result item. 
    /// </summary>
    /// <typeparam name="TSource">The type of source item used in the Iterator.</typeparam>
    /// <typeparam name="TResult">The type of result item provided by the Iterator.</typeparam>
    /// <remarks>
    /// <para>
    /// Whilst Bindable LINQ collections are read-only, we implement the <see cref="T:IList"/> interface because 
    /// it stops WPF producing a wrapper object around the result set, which results in better performance. 
    /// For more information, see:
    /// http://msdn2.microsoft.com/en-gb/library/aa970683.aspx#data_binding.
    /// </para>
    /// </remarks>
    public abstract class Iterator<TSource, TResult> : IBindableQuery<TResult>, IList, IAcceptsDependencies
        where TSource : class
    {
        private readonly StateScope _collectionChangedSuspendedState;
        private readonly List<IDependency> _dependencies;
        private readonly StateScope _isLoadingState;
        private readonly object _iteratorLock = new object();
        private readonly BindableCollection<TResult> _resultCollection;
        private readonly IBindableCollectionInterceptor<TSource> _sourceCollection;
        private readonly EventHandler<NotifyCollectionChangedEventArgs> _sourceCollection_CollectionChanged;
        private readonly EventHandler<PropertyChangedEventArgs> _sourceCollection_PropertyChanged;
        private readonly CollectionChangeObserver _sourceCollectionChangedObserver;
        private readonly PropertyChangeObserver _sourceCollectionPropertyChangedObserver;
        private LoadState _sourceCollectionState;

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator&lt;TSource, TResult&gt;"/> class.
        /// </summary>
        protected Iterator(IBindableCollection<TSource> sourceCollection)
        {
            _dependencies = new List<IDependency>();

            _sourceCollection_CollectionChanged = SourceCollection_CollectionChanged;
            _sourceCollection_PropertyChanged = SourceCollection_PropertyChanged;

            _isLoadingState = new StateScope(delegate { OnPropertyChanged(PropertyChangedCache.IsLoading); });
            _collectionChangedSuspendedState = new StateScope();
            _resultCollection = new BindableCollection<TResult>();
            _resultCollection.CollectionChanged += ResultCollection_CollectionChanged;

            _sourceCollectionState = LoadState.IfNotAlreadyLoaded;

            _sourceCollection = new BindableCollectionInterceptor<TSource>(sourceCollection);
            _sourceCollectionChangedObserver = new CollectionChangeObserver(_sourceCollection_CollectionChanged);
            _sourceCollectionPropertyChangedObserver = new PropertyChangeObserver(_sourceCollection_PropertyChanged);
            _sourceCollectionChangedObserver.Attach(_sourceCollection);
            _sourceCollectionPropertyChangedObserver.Attach(_sourceCollection);
        }

        /// <summary>
        /// Gets a state scope that can be entered and left to indicate the Iterator is currently loading
        /// items.
        /// </summary>
        protected StateScope IsLoadingState
        {
            get { return _isLoadingState; }
        }

        /// <summary>
        /// Gets a state scope that can be entered and left to indicate the Iterator should not raise
        /// CollectionChanged events.
        /// </summary>
        protected StateScope CollectionChangedSuspendedState
        {
            get { return _collectionChangedSuspendedState; }
        }

        /// <summary>
        /// The result collection exposed by the Iterator.
        /// </summary>
        protected BindableCollection<TResult> ResultCollection
        {
            get { return _resultCollection; }
        }

        /// <summary>
        /// Gets the source collection.
        /// </summary>
        protected IBindableCollectionInterceptor<TSource> SourceCollection
        {
            get { return _sourceCollection; }
        }

        /// <summary>
        /// Gets the source collection state.
        /// </summary>
        private LoadState SourceCollectionState
        {
            get { return _sourceCollectionState; }
        }

        /// <summary>
        /// Gets an object used for a monitor whenever we need to lock in this class or derived classes. 
        /// </summary>
        protected object IteratorLock
        {
            get { return _iteratorLock; }
        }

        #region IBindableQuery<TResult> Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            DisposeOverride();
            _sourceCollectionPropertyChangedObserver.Dispose();
            _sourceCollectionChangedObserver.Dispose();
            lock (IteratorLock)
            {
                foreach (var dependency in _dependencies)
                {
                    dependency.Dispose();
                }
            }
            _sourceCollection.Dispose();
        }

        /// <summary>
        /// Gets a value indicating whether this Iterator is currently retrieving items from any of its
        /// source collections.
        /// </summary>
        public bool IsLoading
        {
            get
            {
                var loading = IsLoadingState.IsWithin;
                if (!loading)
                {
                    var loadable = SourceCollection as ILoadable;
                    if (loadable != null)
                    {
                        if (loadable.IsLoading)
                        {
                            loading = true;
                        }
                    }
                }
                return loading;
            }
        }

        /// <summary>
        /// Gets the number of items that are currently available in the result set.
        /// </summary>
        /// <value></value>
        public int CurrentCount
        {
            get { return ResultCollection.Count; }
        }

        /// <summary>
        /// Gets a count of the number of items in this Iterator.
        /// </summary>
        public int Count
        {
            get
            {
                EnsureLoaded(LoadState.IfNotAlreadyLoaded);
                return ResultCollection.Count;
            }
        }

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        public TResult this[int index]
        {
            get
            {
                EnsureLoaded(LoadState.IfNotAlreadyLoaded);
                return ResultCollection[index];
            }
            set { throw new NotSupportedException("This collection is read-only."); }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IBindingConfiguration Configuration
        {
            get
            {
                var result = BindingConfigurations.Default;
                if (SourceCollection is IConfigurable)
                {
                    result = ((IConfigurable) SourceCollection).Configuration;
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
        public IEnumerator<TResult> GetEnumerator()
        {
            EnsureLoaded(LoadState.IfNotAlreadyLoaded);
            return ResultCollection.GetEnumerator();
        }

        /// <summary>
        /// Refreshes all of the source collections.
        /// </summary>
        public void Refresh()
        {
            // First, find out whether or not the source collection we have can be refreshed.
            var isRefreshable = true;
            var alreadyLoaded = false;
            lock (IteratorLock)
            {
                var loadState = SourceCollectionState;
                if (loadState == LoadState.EvenIfLoaded)
                {
                    alreadyLoaded = true;
                    var refreshable = SourceCollection as IRefreshable;
                    if (refreshable == null)
                    {
                        isRefreshable = false;
                    }
                }
            }

            if (alreadyLoaded)
            {
                if (isRefreshable)
                {
                    // The source collections can be refreshed
                    var refreshable = (IRefreshable) SourceCollection;
                    refreshable.Refresh();
                }
                else
                {
                    // Not all sources could be refreshed. The best bet is to reset the iterator - this will 
                    // trigger a reload of all sources instead.
                    Reset();
                }
            }
        }

        /// <summary>
        /// Occurs when the items in this Iterator change.
        /// </summary>
        /// <remarks>Warning: No locks should be held when raising this event.</remarks>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs when a property on this Iterator changes.
        /// </summary>
        /// <remarks>Warning: No locks should be held when raising this event.</remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Accepts a dependency.
        /// </summary>
        /// <param name="definition">The definition.</param>
        public void AcceptDependency(IDependencyDefinition definition)
        {
            if (!definition.AppliesToCollections())
            {
                return;
            }
            var dependency = definition.ConstructForCollection(SourceCollection, Configuration.CreatePathNavigator());
            dependency.SetReevaluateElementCallback(((element, propertyName) => ReactToItemPropertyChanged((TSource) element, propertyName)));
            dependency.SetReevaluateCallback(((element) => Reset()));

            lock (IteratorLock)
            {
                _dependencies.Add(dependency);
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
        #endregion

        #region IList Members
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to add to the <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// The position into which the new element was inserted.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        public int Add(object value)
        {
            throw new NotSupportedException("This collection is read-only.");
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to locate in the <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Object"/> is found in the <see cref="T:System.Collections.IList"/>; otherwise, false.
        /// </returns>
        public bool Contains(object value)
        {
            if (value is TResult)
            {
                EnsureLoaded(LoadState.IfNotAlreadyLoaded);
                lock (IteratorLock)
                {
                    return ResultCollection.Contains((TResult) value);
                }
            }
            return false;
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to locate in the <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// The index of <paramref name="value"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(object value)
        {
            if (value is TResult)
            {
                EnsureLoaded(LoadState.IfNotAlreadyLoaded);
                lock (IteratorLock)
                {
                    return ResultCollection.IndexOf((TResult) value);
                }
            }
            return -1;
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted.</param>
        /// <param name="value">The <see cref="T:System.Object"/> to insert into the <see cref="T:System.Collections.IList"/>.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        /// <exception cref="T:System.NullReferenceException">
        /// 	<paramref name="value"/> is null reference in the <see cref="T:System.Collections.IList"/>.</exception>
        public void Insert(int index, object value)
        {
            throw new NotSupportedException("This collection is read-only.");
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.</returns>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to remove from the <see cref="T:System.Collections.IList"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        public void Remove(object value)
        {
            throw new NotSupportedException("This collection is read-only.");
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.IList"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException("This collection is read-only.");
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        /// <value></value>
        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException("This collection is read-only."); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IList"/> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="array"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="array"/> is multidimensional.-or- <paramref name="index"/> is equal to or greater than the length of <paramref name="array"/>.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>. </exception>
        /// <exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>. </exception>
        public void CopyTo(Array array, int index)
        {
            EnsureLoaded(LoadState.IfNotAlreadyLoaded);
            lock (IteratorLock)
            {
                ResultCollection.CopyTo(array, index);
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.</returns>
        public bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.</returns>
        public object SyncRoot
        {
            get { return new object(); }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only. </exception>
        public void Clear()
        {
            throw new NotSupportedException("This collection is read-only.");
        }
        #endregion

        /// <summary>
        /// When overridden in a derived class, processes a Replace event over a range of items.
        /// </summary>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newItems">The new items.</param>
        protected abstract void ReactToReplaceRange(IEnumerable<TSource> oldItems, IEnumerable<TSource> newItems);

        /// <summary>
        /// When overridden in a derived class, processes a PropertyChanged event on a source item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected abstract void ReactToItemPropertyChanged(TSource item, string propertyName);

        /// <summary>
        /// Processes all source collections.
        /// </summary>
        /// <param name="loadState">
        /// If LoadOption.UnloadedOnly, only collections which haven't already been loaded will 
        /// be loaded. Otherwise, all collections will be forcibly loaded.
        /// </param>
        protected void EnsureLoaded(LoadState? loadState)
        {
            using (CollectionChangedSuspendedState.Enter())
            {
                var performLoad = false;
                lock (IteratorLock)
                {
                    if (loadState == null || loadState == SourceCollectionState)
                    {
                        _sourceCollectionState = LoadState.EvenIfLoaded;
                        performLoad = true;
                    }
                }
                if (!performLoad)
                {
                    return;
                }

                // Note that we should not hold any locks at this point
                using (IsLoadingState.Enter())
                {
                    LoadSourceCollection();
                }
            }
        }

        /// <summary>
        /// Resets the result collection of the Iterator and re-reads all of the source collections.
        /// </summary>
        protected void Reset()
        {
            // Within this block we don't want to allow CollectionChanged events to be raised, 
            // because they may be raised multiple times. Instead, we'll suppress them and raise 
            // a reset event when finished.
            using (CollectionChangedSuspendedState.Enter())
            {
                lock (IteratorLock)
                {
                    ResultCollection.Clear();
                    ResetOverride();
                }
                EnsureLoaded(LoadState.EvenIfLoaded);
            }
            // Now we've reset ourselves, it's time to reload.
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// When implemented in a derived class, processes all items in a given source collection.
        /// </summary>
        /// <remarks>Warning: No locks should be held when invoking this method.</remarks>
        protected abstract void LoadSourceCollection();

        /// <summary>
        /// When overridden in a derived class, processes an Add event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="addedItems">The added items.</param>
        protected abstract void ReactToAddRange(int sourceStartingIndex, IEnumerable<TSource> addedItems);

        /// <summary>
        /// When overridden in a derived class, processes a Move event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="movedItems">The moved items.</param>
        protected abstract void ReactToMoveRange(int sourceStartingIndex, IEnumerable<TSource> movedItems);

        /// <summary>
        /// When overridden in a derived class, processes a Remove event over a range of items.
        /// </summary>
        /// <param name="removedItems">The removed items.</param>
        protected abstract void ReactToRemoveRange(IEnumerable<TSource> removedItems);

        /// <summary>
        /// When overridden in a derived class, provides the derived class with the ability to perform custom actions when 
        /// the collection is reset, before the sources are re-loaded.
        /// </summary>
        /// <remarks>Warning: No locks should be held when invoking this method.</remarks>
        protected virtual void ResetOverride() {}

        private void SourceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // We do not handle CollectionChanged events from sources that are not loaded yet. 
            // Check whether this is a valid source collection.
            var currentState = SourceCollectionState;
            if (sender != SourceCollection || currentState == LoadState.IfNotAlreadyLoaded)
            {
                return;
            }

            // Reset or process the collection changed event. Note that no locks should be held at this 
            // time.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ReactToAddRange(e.NewStartingIndex, e.NewItems.Cast<TSource>());
                    break;
#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
                    ReactToMoveRange(e.NewStartingIndex, e.OldItems.Cast<TSource>());
                    break;
#endif
                case NotifyCollectionChangedAction.Remove:
                    ReactToRemoveRange(e.OldItems.Cast<TSource>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ReactToReplaceRange(e.OldItems.Cast<TSource>(), e.NewItems.Cast<TSource>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
            }
        }

        private void SourceCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // We should bubble up "IsLoading" property changes, because a change in the IsLoading
            // state of a source collection means a change in our own IsLoading state.
            if (e.PropertyName == PropertyChangedCache.IsLoading.PropertyName)
            {
                OnPropertyChanged(PropertyChangedCache.IsLoading);
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <remarks>Warning: No locks should be held when invoking this method.</remarks>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}<{1},{2}> - CurrentCount: {3}", GetType().Name, typeof (TSource).Name, typeof (TResult).Name, CurrentCount);
        }

        private void ResultCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Warning: No locks should be held when invoking this method.</remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Check whether we've been told to suppress CollectionChanged events or not. This 
            // avoids us raising CollectionChanged events during the wrong times - such as 
            // when the collection is first enumerating (a bug in some UI code - namely WPF) or 
            // when resetting the collection.
            if (!CollectionChangedSuspendedState.IsWithin)
            {
                var handler = CollectionChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
                OnPropertyChanged(PropertyChangedCache.Count);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Warning: No locks should be held when invoking this method.</remarks>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// When overridden in a derived class, gives the class an opportunity to dispose any expensive components.
        /// </summary>
        protected virtual void DisposeOverride() {}
    }
}