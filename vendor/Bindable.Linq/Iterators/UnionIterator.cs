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
    /// The Iterator created when a Union is performed.
    /// </summary>
    /// <typeparam name="TElement">The type of source item.</typeparam>
    internal sealed class UnionIterator<TElement> : IBindableQuery<TElement>
        where TElement : class
    {
        private readonly StateScope _collectionChangedSuspendedState;
        private readonly List<IDependency> _dependencies;
        private readonly StateScope _isLoadingState;
        private readonly object _iteratorLock = new object();
        private readonly BindableCollection<TElement> _resultCollection;
        private readonly EventHandler<NotifyCollectionChangedEventArgs> _sourceCollection_CollectionChanged;
        private readonly EventHandler<PropertyChangedEventArgs> _sourceCollection_PropertyChanged;
        private readonly ElementActioner<IBindableCollection<TElement>> _sourceCollectionAddActioner;
        private readonly CollectionChangeObserver _sourceCollectionChangedObserver;
        private readonly PropertyChangeObserver _sourceCollectionPropertyChangedObserver;
        private readonly BindableCollection<IBindableCollection<TElement>> _sourceCollections;
        private readonly StateManager<IBindableCollection<TElement>, LoadState> _sourceCollectionStates;

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator&lt;TElement, TElement&gt;"/> class.
        /// </summary>
        private UnionIterator()
        {
            _dependencies = new List<IDependency>();

            _sourceCollection_CollectionChanged = SourceCollection_CollectionChanged;
            _sourceCollection_PropertyChanged = SourceCollection_PropertyChanged;
            _sourceCollectionChangedObserver = new CollectionChangeObserver(_sourceCollection_CollectionChanged);
            _sourceCollectionPropertyChangedObserver = new PropertyChangeObserver(_sourceCollection_PropertyChanged);

            _isLoadingState = new StateScope(delegate { OnPropertyChanged(PropertyChangedCache.IsLoading); });
            _collectionChangedSuspendedState = new StateScope();
            _sourceCollections = new BindableCollection<IBindableCollection<TElement>>();
            _sourceCollectionStates = new StateManager<IBindableCollection<TElement>, LoadState>();
            _resultCollection = new BindableCollection<TElement>();
            _resultCollection.CollectionChanged += ResultCollection_CollectionChanged;

            _sourceCollectionAddActioner = new ElementActioner<IBindableCollection<TElement>>(_sourceCollections, enumerable => SourceCollectionAdded(enumerable), enumerable => SourceCollectionRemoved(enumerable));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator&lt;TElement, TElement&gt;"/> class.
        /// </summary>
        /// <param name="sourceCollections">The source collections.</param>
        public UnionIterator(IEnumerable<IBindableCollection<TElement>> sourceCollections)
            : this()
        {
            SourceCollections.AddRange(sourceCollections);
        }

        /// <summary>
        /// Gets a state scope that can be entered and left to indicate the Iterator is currently loading
        /// items.
        /// </summary>
        private StateScope IsLoadingState
        {
            get { return _isLoadingState; }
        }

        /// <summary>
        /// Gets a state scope that can be entered and left to indicate the Iterator should not raise
        /// CollectionChanged events.
        /// </summary>
        private StateScope CollectionChangedSuspendedState
        {
            get { return _collectionChangedSuspendedState; }
        }

        /// <summary>
        /// The result collection exposed by the Iterator.
        /// </summary>
        private BindableCollection<TElement> ResultCollection
        {
            get { return _resultCollection; }
        }

        /// <summary>
        /// Gets the source collections.
        /// </summary>
        private BindableCollection<IBindableCollection<TElement>> SourceCollections
        {
            get { return _sourceCollections; }
        }

        /// <summary>
        /// Gets the source collection state manager.
        /// </summary>
        /// <value>The source collection states.</value>
        private StateManager<IBindableCollection<TElement>, LoadState> SourceCollectionStates
        {
            get { return _sourceCollectionStates; }
        }

        /// <summary>
        /// Gets an object used for a monitor whenever we need to lock in this class or derived classes. 
        /// Note that this lock will use a timeout when locked, and so any code may throw 
        /// <see cref="T:LockAttemptTimeoutException">LockAttemptTimeoutExceptions</see>.
        /// </summary>
        private object IteratorLock
        {
            get { return _iteratorLock; }
        }

        #region IBindableQuery<TElement> Members
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
                    // Check to see whether any of the source collections are loading.
                    var sourceCollections = SourceCollections.GetEnumerator();
                    while (sourceCollections.MoveNext())
                    {
                        var loadable = sourceCollections.Current as ILoadable;
                        if (loadable != null)
                        {
                            if (loadable.IsLoading)
                            {
                                loading = true;
                                break;
                            }
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
        /// Gets the <see cref="T:TElement"/> at the specified index.
        /// </summary>
        /// <value></value>
        public TElement this[int index]
        {
            get { return ResultCollection[index]; }
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
                foreach (var source in SourceCollections)
                {
                    if (source is IConfigurable)
                    {
                        result = ((IConfigurable) source).Configuration;
                        break;
                    }
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
            EnsureLoaded(LoadState.IfNotAlreadyLoaded);
            return ResultCollection.GetEnumerator();
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
        /// Refreshes all of the source collections.
        /// </summary>
        public void Refresh()
        {
            var sourceCollections = SourceCollections.GetEnumerator();

            // First, find out whether or not ALL of the source collections we have can be refreshed.
            var allRefreshable = true;
            var nothingLoadedYet = true;
            lock (IteratorLock)
            {
                while (sourceCollections.MoveNext())
                {
                    var loadState = SourceCollectionStates.GetState(sourceCollections.Current);
                    if (loadState == LoadState.EvenIfLoaded)
                    {
                        nothingLoadedYet = false;
                        var refreshable = sourceCollections.Current as IRefreshable;
                        if (refreshable == null)
                        {
                            allRefreshable = false;
                            break;
                        }
                    }
                }
            }

            if (!nothingLoadedYet)
            {
                if (allRefreshable)
                {
                    // All source collections can be refreshed. Loop again and refresh them
                    sourceCollections.Reset();
                    while (sourceCollections.MoveNext())
                    {
                        var refreshable = (IRefreshable) sourceCollections.Current;
                        refreshable.Refresh();
                    }
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
        /// Sets a new dependency on a Bindable LINQ operation.
        /// </summary>
        /// <param name="definition">A definition of the dependency.</param>
        public void AcceptDependency(IDependencyDefinition definition)
        {
            throw new NotSupportedException("Dependencies are not supported by the Union operator at this stage.");
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            _sourceCollectionPropertyChangedObserver.Dispose();
            _sourceCollectionChangedObserver.Dispose();
        }
        #endregion

        /// <summary>
        /// Processes all source collections.
        /// </summary>
        /// <param name="loadState">If LoadOption.UnloadedOnly, only collections which haven't already been loaded will
        /// be loaded. Otherwise, all collections will be forcibly loaded.</param>
        private void EnsureLoaded(LoadState? loadState)
        {
            var sourceCollections = _sourceCollectionStates.GetAllInState(loadState);
            while (sourceCollections.MoveNext())
            {
                SourceCollectionStates.SetState(sourceCollections.Current, LoadState.EvenIfLoaded);
            }
            sourceCollections.Reset();

            // Note that we should not hold any locks at this point
            using (CollectionChangedSuspendedState.Enter())
            {
                using (IsLoadingState.Enter())
                {
                    while (sourceCollections.MoveNext())
                    {
                        LoadSourceCollection(sourceCollections.Current);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the result collection of the Iterator and re-reads all of the source collections.
        /// </summary>
        private void Reset()
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
        /// <param name="sourceCollection">The collection of source items to process.</param>
        /// <remarks>Warning: No locks should be held when invoking this method.</remarks>
        private void LoadSourceCollection(IEnumerable<TElement> sourceCollection)
        {
            HandleAddRangeOverride(-1, sourceCollection);
        }

        /// <summary>
        /// When overridden in a derived class, processes an Add event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="addedItems">The added items.</param>
        private void HandleAddRangeOverride(int sourceStartingIndex, IEnumerable<TElement> addedItems)
        {
            ResultCollection.AddRange(addedItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a Move event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="movedItems">The moved items.</param>
        private void HandleMoveRangeOverride(int sourceStartingIndex, IEnumerable<TElement> movedItems) {}

        /// <summary>
        /// When overridden in a derived class, processes a Remove event over a range of items.
        /// </summary>
        /// <param name="removedItems">The removed items.</param>
        private void HandleRemoveRangeOverride(IEnumerable<TElement> removedItems)
        {
            ResultCollection.RemoveRange(removedItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a Replace event over a range of items.
        /// </summary>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newItems">The new items.</param>
        private void HandleReplaceRangeOverride(IEnumerable<TElement> oldItems, IEnumerable<TElement> newItems)
        {
            ResultCollection.ReplaceRange(oldItems, newItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a PropertyChanged event on a source item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="propertyName">Name of the property.</param>
        private void HandleItemPropertyChanged(TElement item, string propertyName) {}

        /// <summary>
        /// When overridden in a derived class, provides the derived class with the ability to perform custom actions when 
        /// the collection is reset, before the sources are re-loaded.
        /// </summary>
        /// <remarks>Warning: No locks should be held when invoking this method.</remarks>
        private void ResetOverride() {}

        private void SourceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // We do not handle CollectionChanged events from sources that are not loaded yet. 
            // Check whether this is a valid source collection.
            var sourceCollection = (IBindableCollection<TElement>) sender;
            var currentState = SourceCollectionStates.GetState(sourceCollection);
            if (currentState == null || currentState.Value == LoadState.IfNotAlreadyLoaded)
            {
                return;
            }

            // Reset or process the collection changed event. Note that no locks should be held at this 
            // time.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    HandleAddRangeOverride(e.NewStartingIndex, e.NewItems.Cast<TElement>());
                    break;
#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
                    HandleMoveRangeOverride(e.NewStartingIndex, e.OldItems.Cast<TElement>());
                    break;
#endif
                case NotifyCollectionChangedAction.Remove:
                    HandleRemoveRangeOverride(e.OldItems.Cast<TElement>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    HandleReplaceRangeOverride(e.OldItems.Cast<TElement>(), e.NewItems.Cast<TElement>());
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

        private void SourceCollectionAdded(IBindableCollection<TElement> sourceCollection)
        {
            _sourceCollectionStates.SetState(sourceCollection, LoadState.IfNotAlreadyLoaded);
            _sourceCollectionChangedObserver.Attach(sourceCollection);
            _sourceCollectionPropertyChangedObserver.Attach(sourceCollection);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void SourceCollectionRemoved(IBindableCollection<TElement> sourceCollection)
        {
            _sourceCollectionStates.Remove(sourceCollection);
            _sourceCollectionChangedObserver.Detach(sourceCollection);
            _sourceCollectionPropertyChangedObserver.Detach(sourceCollection);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
            return string.Format(CultureInfo.InvariantCulture, "{0}<{1}> - CurrentCount: {2}", GetType().Name, typeof (TElement).Name, CurrentCount);
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
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
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
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}