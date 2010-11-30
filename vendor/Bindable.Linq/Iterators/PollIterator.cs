namespace Bindable.Linq.Iterators
{
    using System;
    using System.Collections.Generic;
    using Helpers;
    using Threading;

    /// <summary>
    /// An Iterator that reads items from the source collection directly into the results collection, 
    /// and then continues to poll the source collection for changes at a given interval.
    /// </summary>
    /// <typeparam name="TElement">The type of source item.</typeparam>
    internal sealed class PollIterator<TElement> : Iterator<TElement, TElement>
        where TElement : class
    {
        private readonly IDispatcher _dispatcher;
        private readonly Action _reloadCallback;
        private readonly WeakTimer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PollIterator&lt;S&gt;"/> class.
        /// </summary>
        /// <param name="sourceCollection">The source collection.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="pollTime">The poll time.</param>
        public PollIterator(IBindableCollection<TElement> sourceCollection, IDispatcher dispatcher, TimeSpan pollTime)
            : base(sourceCollection)
        {
            _dispatcher = dispatcher;
            _reloadCallback = Timer_Tick;
            _timer = new WeakTimer(pollTime, _reloadCallback);
        }

        /// <summary>
        /// When implemented in a derived class, processes all items in the source collection.
        /// </summary>
        protected override void LoadSourceCollection()
        {
            ReactToAddRange(0, SourceCollection);

            _timer.Start();
        }

        private void Timer_Tick()
        {
            _timer.Pause();
            _dispatcher.Invoke(Reload);
        }

        private void Reload()
        {
            // Read all of the items out of the source collections first before entering any locks. 
            // This avoids deadlock situations occurring where the enumerators of the source collections
            // on another thread need a property on this object locked by this thread whilst we wait 
            // for them to return.
            var allSourceItems = new List<TElement>();
            using (IsLoadingState.Enter())
            {
                foreach (var item in SourceCollection)
                {
                    allSourceItems.Add(item);
                }
            }

            // Now it is safe to acquire a lock and to decide whether to add/remove the items
            using (var transaction = ResultCollection.BeginTransaction())
            {
                lock (IteratorLock)
                {
                    foreach (var item in allSourceItems)
                    {
                        if (!ResultCollection.Contains(item))
                        {
                            ResultCollection.Add(item, transaction);
                        }
                    }
                    foreach (var item in ResultCollection)
                    {
                        if (!allSourceItems.Contains(item))
                        {
                            ResultCollection.Remove(item, transaction);
                        }
                    }
                }
            }

            _timer.Continue();
        }

        /// <summary>
        /// When overridden in a derived class, processes an Add event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="addedItems">The added items.</param>
        protected override void ReactToAddRange(int sourceStartingIndex, IEnumerable<TElement> addedItems)
        {
            ResultCollection.AddOrInsertRange(sourceStartingIndex, addedItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a Move event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="movedItems">The moved items.</param>
        protected override void ReactToMoveRange(int sourceStartingIndex, IEnumerable<TElement> movedItems)
        {
            ResultCollection.MoveRange(sourceStartingIndex, movedItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a Remove event over a range of items.
        /// </summary>
        /// <param name="removedItems">The removed items.</param>
        protected override void ReactToRemoveRange(IEnumerable<TElement> removedItems)
        {
            ResultCollection.RemoveRange(removedItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a Replace event over a range of items.
        /// </summary>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newItems">The new items.</param>
        protected override void ReactToReplaceRange(IEnumerable<TElement> oldItems, IEnumerable<TElement> newItems)
        {
            ResultCollection.ReplaceRange(oldItems, newItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a PropertyChanged event on a source item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected override void ReactToItemPropertyChanged(TElement item, string propertyName) {}

        /// <summary>
        /// When overridden in a derived class, gives the class an opportunity to dispose any expensive components.
        /// </summary>
        protected override void DisposeOverride()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }
}