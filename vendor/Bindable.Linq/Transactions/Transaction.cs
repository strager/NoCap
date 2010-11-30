namespace Bindable.Linq.Transactions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using Helpers;

    /// <summary>
    /// This class abstracts recording of collection changed events and raising them when complete. Events can be 
    /// recorded whilst locks are held, and then raised at once. It automatically figures out whether events are 
    /// contiguous (i.e., you add an item at index 1, then at index two) or not and combines them appropriately.
    /// </summary>
    internal sealed class Transaction : ITransaction
    {
        private readonly Action<TransactionLog> _commitTransactionCallback;
        private readonly List<NotifyCollectionChangedEventArgs> _recorded = new List<NotifyCollectionChangedEventArgs>();
        private readonly object _transactionLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class.
        /// </summary>
        /// <param name="commitTransactionCallback">The commit transaction callback.</param>
        public Transaction(Action<TransactionLog> commitTransactionCallback)
        {
            _commitTransactionCallback = commitTransactionCallback;
        }

        #region ITransaction Members
        /// <summary>
        /// Records the fact that an element was added.
        /// </summary>
        /// <param name="elementAdded"></param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="elementAdded"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
        public void LogAddEvent(object elementAdded, int index)
        {
            elementAdded.ShouldNotBeNull("elementAdded");
            index.ShouldBe(e => (e >= 0), "index");
            Append(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, elementAdded, index));
        }

        /// <summary>
        /// Records the fact that the collection has changed dramatically and should be refreshed.
        /// </summary>
        public void LogResetEvent()
        {
            Append(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Records the fact that an element was moved.
        /// </summary>
        /// <param name="elementMoved"></param>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        /// <exception cref="ArgumentNullException"><paramref name="elementMoved"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="oldIndex"/> is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newIndex"/> is less than zero.</exception>
        public void LogMoveEvent(object elementMoved, int oldIndex, int newIndex)
        {
            elementMoved.ShouldNotBeNull("elementMoved");
            oldIndex.ShouldBe(e => (e >= 0), "oldIndex");
            newIndex.ShouldBe(e => (e >= 0), "newIndex");

#if SILVERLIGHT
    // In Silverlight 2.0 beta 1, Move is not supported.
            this.LogRemoveEvent(elementMoved, oldIndex);
            this.LogAddEvent(elementMoved, newIndex);
#else
            Append(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, elementMoved, newIndex, oldIndex));
#endif
        }

        /// <summary>
        /// Records the fact that an element was removed.
        /// </summary>
        /// <param name="removedElement">The removed element.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="removedElement"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
        public void LogRemoveEvent(object removedElement, int index)
        {
            removedElement.ShouldNotBeNull("removedElement");
            index.ShouldBe(e => (e >= 0), "index");
            Append(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElement, index));
        }

        /// <summary>
        /// Records the fact that an element was replaced.
        /// </summary>
        /// <param name="originalElement">The original element.</param>
        /// <param name="replacementElement">The replacement element.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="originalElement"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="replacementElement"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
        public void LogReplaceEvent(object originalElement, object replacementElement, int index)
        {
            originalElement.ShouldNotBeNull("originalElement");
            replacementElement.ShouldNotBeNull("replacementElement");
            index.ShouldBe(e => (e >= 0), "index");
            Append(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, replacementElement, originalElement, index));
        }

        /// <summary>
        /// Raises the events. It is imperative that the caller does not hold any locks at this point.
        /// </summary>
        public void Commit()
        {
            var eventsToRaise = new List<NotifyCollectionChangedEventArgs>();
            lock (_transactionLock)
            {
                eventsToRaise.AddRange(_recorded);
                _recorded.Clear();
            }
            if (_commitTransactionCallback != null)
            {
                _commitTransactionCallback(new TransactionLog(eventsToRaise));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Commit();
        }
        #endregion

        private void Append(NotifyCollectionChangedEventArgs eventToAppend)
        {
            // This method is only called within this class, so the following assertion should always be true
            Debug.Assert((eventToAppend.NewItems == null || eventToAppend.NewItems.Count == 1) || (eventToAppend.OldItems == null || eventToAppend.OldItems.Count == 1));
            lock (_transactionLock)
            {
                var appended = false;
#if !SILVERLIGHT
                // In Silverlight 2.0 beta 1, events cannot be raised with multiple objects.
                if (eventToAppend.Action != NotifyCollectionChangedAction.Reset && eventToAppend.Action != NotifyCollectionChangedAction.Move)
                {
                    for (var ixEvent = 0; ixEvent < _recorded.Count; ixEvent++)
                    {
                        var existingEvent = _recorded[ixEvent];
                        if (existingEvent.Action == eventToAppend.Action)
                        {
                            var lastIndexOfExistingEvent = -1;
                            var indexOfEventToAppend = -1;
                            if (existingEvent.Action == NotifyCollectionChangedAction.Remove)
                            {
                                lastIndexOfExistingEvent = existingEvent.OldStartingIndex + existingEvent.OldItems.Count - 1;
                                indexOfEventToAppend = eventToAppend.OldStartingIndex + eventToAppend.OldItems.Count - 1;
                            }
                            else if (existingEvent.Action == NotifyCollectionChangedAction.Move)
                            {
                                lastIndexOfExistingEvent = existingEvent.OldStartingIndex + existingEvent.OldItems.Count;
                                indexOfEventToAppend = eventToAppend.OldStartingIndex + eventToAppend.OldItems.Count - 1;
                            }
                            else
                            {
                                lastIndexOfExistingEvent = existingEvent.NewStartingIndex + existingEvent.NewItems.Count;
                                indexOfEventToAppend = eventToAppend.NewStartingIndex + eventToAppend.NewItems.Count - 1;
                            }

                            if (indexOfEventToAppend == lastIndexOfExistingEvent)
                            {
                                var oldStartingIndex = existingEvent.OldStartingIndex;
                                var newStartingIndex = existingEvent.NewStartingIndex;
                                var oldItems = new List<object>();
                                var newItems = new List<object>();
                                if (existingEvent.OldItems != null)
                                {
                                    oldItems.AddRange(existingEvent.OldItems.Cast<object>());
                                    oldItems.Add(eventToAppend.OldItems[0]);
                                }
                                if (existingEvent.NewItems != null)
                                {
                                    newItems.AddRange(existingEvent.NewItems.Cast<object>());
                                    newItems.Add(eventToAppend.NewItems[0]);
                                }

                                switch (existingEvent.Action)
                                {
                                    case NotifyCollectionChangedAction.Add:
                                        _recorded[ixEvent] = new NotifyCollectionChangedEventArgs(existingEvent.Action, newItems, newStartingIndex);
                                        break;
                                    case NotifyCollectionChangedAction.Move:
                                        _recorded[ixEvent] = new NotifyCollectionChangedEventArgs(existingEvent.Action, newItems, newStartingIndex, oldStartingIndex);
                                        break;
                                    case NotifyCollectionChangedAction.Remove:
                                        _recorded[ixEvent] = new NotifyCollectionChangedEventArgs(existingEvent.Action, oldItems, oldStartingIndex);
                                        break;
                                    case NotifyCollectionChangedAction.Replace:
                                        _recorded[ixEvent] = new NotifyCollectionChangedEventArgs(existingEvent.Action, newItems, oldItems, newStartingIndex);
                                        break;
                                }
                                appended = true;
                                break;
                            }
                        }
                    }
                }
#endif
                if (!appended)
                {
                    _recorded.Add(eventToAppend);
                }
            }
        }
    }
}