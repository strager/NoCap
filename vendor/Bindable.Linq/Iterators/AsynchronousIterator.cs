namespace Bindable.Linq.Iterators
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Helpers;
    using Threading;

    /// <summary>
    /// An Iterator that reads the the enumerator of the source collection on a background thread, 
    /// using the advantages of data binding and <see cref="T:INotifyCollectionChanged"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of source item.</typeparam>
    internal sealed class AsynchronousIterator<TElement> : Iterator<TElement, TElement>
        where TElement : class
    {
        private readonly IDispatcher _dispatcher;
        private IteratorThread _iteratorThread;
        private StateScope _loadingState;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AsynchronousIterator`1"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        public AsynchronousIterator(IBindableCollection<TElement> source, IDispatcher dispatcher)
            : base(source)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// When implemented in a derived class, processes all items in the <see cref="P:SourceCollection"/>.
        /// </summary>
        protected override void LoadSourceCollection()
        {
            if (_loadingState != null)
            {
                _loadingState.Leave();
            }

            _loadingState = IsLoadingState.Enter();
            lock (IteratorLock)
            {
                if (_iteratorThread != null)
                {
                    _iteratorThread.Cancel = true;
                }
                _iteratorThread = new IteratorThread();
            }
            _iteratorThread.SourceCollection = SourceCollection;
            _iteratorThread.YieldCallback = delegate(TElement element) { _dispatcher.Invoke(() => ReactToAddRange(0, new TElement[] {element})); };
            _iteratorThread.CompletedCallback = delegate
            {
                _dispatcher.Invoke(() =>
                {
                    if (_loadingState != null)
                    {
                        _loadingState.Leave();
                    }
                });
            };

            var t = new Thread(_iteratorThread.Iterate);
            t.Start();
        }

        /// <summary>
        /// When overridden in a derived class, processes an Add event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="addedItems">The added items.</param>
        protected override void ReactToAddRange(int sourceStartingIndex, IEnumerable<TElement> addedItems)
        {
            var elementsToAdd = addedItems.EnumerateSafely();
            using (var transaction = ResultCollection.BeginTransaction())
            {
                lock (IteratorLock)
                {
                    foreach (var element in elementsToAdd)
                    {
                        if (!ResultCollection.Contains(element))
                        {
                            ResultCollection.Add(element, transaction);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, processes a Move event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="movedItems">The moved items.</param>
        protected override void ReactToMoveRange(int sourceStartingIndex, IEnumerable<TElement> movedItems) {}

        /// <summary>
        /// When overridden in a derived class, processes a Remove event over a range of items.
        /// </summary>
        /// <param name="removedItems">The removed items.</param>
        protected override void ReactToRemoveRange(IEnumerable<TElement> removedItems)
        {
            var elementsToRemove = removedItems.EnumerateSafely();
            using (var transaction = ResultCollection.BeginTransaction())
            {
                lock (IteratorLock)
                {
                    ResultCollection.RemoveRange(elementsToRemove, transaction);
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, processes a Replace event over a range of items.
        /// </summary>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newItems">The new items.</param>
        protected override void ReactToReplaceRange(IEnumerable<TElement> oldItems, IEnumerable<TElement> newItems)
        {
            var oldElements = oldItems.EnumerateSafely();
            var newElements = newItems.EnumerateSafely();
            using (var transaction = ResultCollection.BeginTransaction())
            {
                lock (IteratorLock)
                {
                    ResultCollection.ReplaceRange(oldElements, newElements, new List<int>(), transaction);
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, processes a PropertyChanged event on a source item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected override void ReactToItemPropertyChanged(TElement item, string propertyName)
        {
            // Nothing to do here
        }

        #region Nested type: IteratorThread
        private class IteratorThread
        {
            public bool Cancel { get; set; }

            public IBindableCollection<TElement> SourceCollection { get; set; }

            public Action<TElement> YieldCallback { get; set; }
            public Action CompletedCallback { get; set; }

            public void Iterate(object state)
            {
                var enumerator = SourceCollection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (Cancel)
                    {
                        break;
                    }
                    if (current != null)
                    {
                        if (YieldCallback != null)
                        {
                            YieldCallback(current);
                        }
                    }
                }
                CompletedCallback();
            }
        }
        #endregion
    }
}