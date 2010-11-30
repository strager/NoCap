namespace Bindable.Linq.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading;
    using Helpers;
    using Transactions;

    /// <summary>
    /// This class is used as the primary implementation of a bindable collection. Most of the Iterators
    /// in Bindable LINQ use this class eventually to store their bindable results, as it abstracts a lot of the 
    /// logic around adding, replacing, moving and removing collections of items and raising the correct 
    /// events. It is similar to the <see cref="T:ObservableCollection`1"/> in most ways, but provides 
    /// additional functionality.
    /// </summary>
    /// <typeparam name="TElement">The type of item held within the collection.</typeparam>
    public class BindableCollection<TElement> : IBindableCollection<TElement>, IList<TElement>, IEnumerable<TElement>, INotifyCollectionChanged, INotifyPropertyChanged, IList, IBindableCollection, IDisposable
    {
        private static readonly PropertyChangedEventArgs CountPropertyChangedEventArgs = new PropertyChangedEventArgs("Count");
        private readonly object _bindableCollectionLock = new object();
        private readonly IEqualityComparer<TElement> _comparer = ElementComparerFactory.Create<TElement>();
        private readonly List<TElement> _innerList;
        private readonly SnapshotManager<TElement> _snapshotManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BindableCollection`1"/> class.
        /// </summary>
        public BindableCollection()
        {
            _innerList = new List<TElement>();
            _snapshotManager = new SnapshotManager<TElement>(RebuildSnapshotCallback);
        }

        /// <summary>
        /// Gets the item used to lock this collection.
        /// </summary>
        /// <remarks>
        /// TODO: This should not be internal.
        /// </remarks>
        protected object BindableCollectionLock
        {
            get { return _bindableCollectionLock; }
        }

        /// <summary>
        /// Gets the <see cref="T:List`1"/> used internally to store the items.
        /// </summary>
        private List<TElement> InnerList
        {
            get { return _innerList; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has property changed subscribers.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has property changed subscribers; otherwise, <c>false</c>.
        /// </value>
        internal bool HasPropertyChangedSubscribers
        {
            get { return PropertyChanged != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has collection changed subscribers.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has collection changed subscribers; otherwise, <c>false</c>.
        /// </value>
        internal bool HasCollectionChangedSubscribers
        {
            get { return CollectionChanged != null; }
        }

        #region Add
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(TElement item)
        {
            AddRange(new[] {item});
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <param name="transaction">The transaction.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(TElement item, ITransaction transaction)
        {
            AddRange(new[] {item}, transaction);
        }

        /// <summary>
        /// Adds a range of items to the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="range">The items to add.</param>
        public void AddRange(params TElement[] range)
        {
            AddRange((IEnumerable<TElement>) range);
        }

        /// <summary>
        /// Adds a range of items to the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="range">The items to add.</param>
        public void AddRange(IEnumerable<TElement> range)
        {
            using (var transaction = BeginTransaction())
            {
                AddRange(range, transaction);
            }
        }

        /// <summary>
        /// Adds a range of items to the collection (whilst holding a lock) without raising any collection changed events.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="transaction">The transaction.</param>
        public void AddRange(IEnumerable<TElement> range, ITransaction transaction)
        {
            var itemsToAdd = range.EnumerateSafely();

            lock (BindableCollectionLock)
            {
                foreach (var item in itemsToAdd)
                {
                    var index = ((IList) InnerList).Add(item);
                    _snapshotManager.Invalidate();
                    transaction.LogAddEvent(item, index);
                }
            }
        }

        /// <summary>
        /// Adds or inserts a range of items at a given index (which may be negative, in which case 
        /// the items will be added (appended to the end).
        /// </summary>
        /// <param name="index">The index to add the items at.</param>
        /// <param name="items">The items to add.</param>
        public void AddOrInsertRange(int index, IEnumerable<TElement> items)
        {
            if (index == -1)
            {
                AddRange(items);
            }
            else
            {
                InsertRange(index, items);
            }
        }
        #endregion

        #region Insert
        /// <summary>
        /// Inserts an item to the <see cref="T:BindableCollection`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:BindableCollection`1"/>.</param>
        public void Insert(int index, TElement item)
        {
            InsertRange(index, new[] {item});
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:BindableCollection`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:BindableCollection`1"/>.</param>
        /// <param name="transaction">The transaction.</param>
        public void Insert(int index, TElement item, ITransaction transaction)
        {
            InsertRange(index, new[] {item}, transaction);
        }

        /// <summary>
        /// Inserts a range of items into the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="index">The index to start inserting at.</param>
        /// <param name="range">The items to insert into the <see cref="T:BindableCollection`1"/>.</param>
        public void InsertRange(int index, IEnumerable<TElement> range)
        {
            using (var transaction = BeginTransaction())
            {
                InsertRange(index, range, transaction);
            }
        }

        /// <summary>
        /// Inserts a range of items into the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="index">The index to start inserting at.</param>
        /// <param name="range">The items to insert into the <see cref="T:BindableCollection`1"/>.</param>
        /// <param name="transaction">The transaction.</param>
        public void InsertRange(int index, IEnumerable<TElement> range, ITransaction transaction)
        {
            if (index < 0)
            {
                AddRange(range, transaction);
                return;
            }

            var itemsToInsert = range.EnumerateSafely();

            lock (BindableCollectionLock)
            {
                if (index > InnerList.Count)
                {
                    index = InnerList.Count;
                }

                for (var ixCurrentItem = 0; ixCurrentItem < itemsToInsert.Count; ixCurrentItem++)
                {
                    var insertionIndex = index + ixCurrentItem;

                    InnerList.Insert(insertionIndex, itemsToInsert[ixCurrentItem]);
                    _snapshotManager.Invalidate();

                    transaction.LogAddEvent(itemsToInsert[ixCurrentItem], insertionIndex);
                }
            }
        }

        /// <summary>
        /// Inserts a range of items so that they appear in order, using a given comparer.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="comparer">The comparer.</param>
        public void InsertRangeOrder(IEnumerable<TElement> range, Comparison<TElement> comparer)
        {
            using (var transaction = BeginTransaction())
            {
                InsertRangeOrder(range, comparer, transaction);
            }
        }

        /// <summary>
        /// Inserts a range of items so that they appear in order, using a given comparer.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="transaction">The transaction.</param>
        public void InsertRangeOrder(IEnumerable<TElement> range, Comparison<TElement> comparer, ITransaction transaction)
        {
            var itemsToInsert = range.EnumerateSafely();

            lock (BindableCollectionLock)
            {
                foreach (var element in itemsToInsert)
                {
                    var inserted = false;
                    for (var i = 0; i < InnerList.Count; i++)
                    {
                        var result = comparer(element, InnerList[i]);
                        if (result <= 0)
                        {
                            Insert(i, element, transaction);
                            inserted = true;
                            break;
                        }
                    }
                    if (!inserted)
                    {
                        Add(element, transaction);
                    }
                }
            }
        }
        #endregion

        #region Move
        /// <summary>
        /// Moves an item to a new location within the collection.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        /// <param name="item">The item to move.</param>
        public void Move(int newIndex, TElement item)
        {
            MoveRange(newIndex, new[] {item});
        }

        /// <summary>
        /// Moves an item to a new location within the collection.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        /// <param name="item">The item to move.</param>
        /// <param name="transaction">The transaction.</param>
        public void Move(int newIndex, TElement item, ITransaction transaction)
        {
            MoveRange(newIndex, new[] {item}, transaction);
        }

        /// <summary>
        /// Moves a collection of items from their old location (whether the items are contiguous or not) 
        /// to a new starting location (where the items will be contiguous).
        /// </summary>
        /// <param name="range">The items to move.</param>
        /// <param name="newIndex">The new index to move the items to.</param>
        public void MoveRange(int newIndex, IEnumerable<TElement> range)
        {
            using (var transaction = BeginTransaction())
            {
                MoveRange(newIndex, range, transaction);
            }
        }

        /// <summary>
        /// Moves a collection of items from their old location (whether the items are contiguous or not)
        /// to a new starting location (where the items will be contiguous).
        /// </summary>
        /// <param name="newIndex">The new index to move the items to.</param>
        /// <param name="range">The items to move.</param>
        /// <param name="transaction">The transaction.</param>
        /// <remarks>
        /// Here is an example of how this logic works:
        /// Index     Start        Step 1       Step 2
        /// --------------------------------------------
        /// 0:        Paul         Paul         Paul
        /// 1:        Chuck        Larry        Larry
        /// 2:        Larry        Timone       Timone
        /// 3:        Timone       Pumba        Pumba
        /// 4:        Pumba        Patrick      Chuck
        /// 5:        Patrick                   Patrick
        /// Operation: Move "Chuck" from ix=1 to ix=4
        /// 1) Remove "Chuck" - removedIndex = 1
        /// 2) Insert "Chuck" - newIndex = 4
        /// </remarks>
        public void MoveRange(int newIndex, IEnumerable<TElement> range, ITransaction transaction)
        {
            var itemsToMove = range.EnumerateSafely();

            lock (BindableCollectionLock)
            {
                for (var ixCurrentItem = 0; ixCurrentItem < itemsToMove.Count; ixCurrentItem++)
                {
                    var element = itemsToMove[ixCurrentItem];
                    var originalIndex = IndexOf(element);
                    var desiredIndex = newIndex + ixCurrentItem;

                    // Remove it temporarily
                    var removed = false;
                    if (originalIndex >= 0)
                    {
                        InnerList.Remove(element);
                        removed = true;
                    }

                    // Insert it into the correct spot
                    if (desiredIndex >= InnerList.Count)
                    {
                        desiredIndex = ((IList) InnerList).Add(element);
                    }
                    else
                    {
                        InnerList.Insert(desiredIndex, element);
                    }
                    _snapshotManager.Invalidate();

                    // Record the appropriate event
                    if (removed)
                    {
                        transaction.LogMoveEvent(element, originalIndex, desiredIndex);
                    }
                    else
                    {
                        transaction.LogAddEvent(element, desiredIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Moves an item to the correct place if it is no longer in the right place.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="comparer">The comparer.</param>
        public void MoveItemOrdered(TElement item, Comparison<TElement> comparer)
        {
            using (var transaction = BeginTransaction())
            {
                MoveOrdered(item, comparer, transaction);
            }
        }

        /// <summary>
        /// Moves an item to the correct place if it is no longer in the right place.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="transaction">The transaction.</param>
        public void MoveOrdered(TElement element, Comparison<TElement> comparer, ITransaction transaction)
        {
            lock (BindableCollectionLock)
            {
                var originalIndex = IndexOf(element);
                if (originalIndex >= 0)
                {
                    for (var i = 0; i < InnerList.Count; i++)
                    {
                        var result = comparer(element, InnerList[i]);
                        if (result <= 0)
                        {
                            var itemAlreadyInOrder = true;
                            for (var j = i; j < originalIndex && j < InnerList.Count; j++)
                            {
                                if (comparer(element, InnerList[j]) > 0)
                                {
                                    itemAlreadyInOrder = false;
                                    break;
                                }
                            }

                            if (!itemAlreadyInOrder)
                            {
                                if (i != originalIndex)
                                {
                                    Move(i, element, transaction);
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Replace
        /// <summary>
        /// Replaces a given item with another item.
        /// </summary>
        /// <param name="oldItem">The old item.</param>
        /// <param name="newItem">The new item.</param>
        public void Replace(TElement oldItem, TElement newItem)
        {
            ReplaceRange(new[] {oldItem}, new[] {newItem});
        }

        /// <summary>
        /// Replaces a given item with another item.
        /// </summary>
        /// <param name="oldItem">The old item.</param>
        /// <param name="newItem">The new item.</param>
        /// <param name="transaction">The transaction.</param>
        public void Replace(TElement oldItem, TElement newItem, ITransaction transaction)
        {
            ReplaceRange(new[] {oldItem}, new[] {newItem});
        }

        /// <summary>
        /// Replaces a list of old items with a list of new items.
        /// </summary>
        /// <param name="oldItemsRange">The old items range.</param>
        /// <param name="newItemsRange">The new items range.</param>
        public void ReplaceRange(IEnumerable<TElement> oldItemsRange, IEnumerable<TElement> newItemsRange)
        {
            ReplaceRange(oldItemsRange, newItemsRange, new List<int>());
        }

        /// <summary>
        /// Replaces a list of old items with a list of new items.
        /// </summary>
        /// <param name="oldItemsRange">The old items range.</param>
        /// <param name="newItemsRange">The new items range.</param>
        /// <param name="newItemsToSkip">The new items to skip.</param>
        /// <remarks>
        /// TODO: newItemsToSkip is a HACK. Should find a better approach.
        /// </remarks>
        public void ReplaceRange(IEnumerable<TElement> oldItemsRange, IEnumerable<TElement> newItemsRange, List<int> newItemsToSkip)
        {
            using (var transaction = BeginTransaction())
            {
                ReplaceRange(oldItemsRange, newItemsRange, newItemsToSkip, transaction);
            }
        }

        /// <summary>
        /// Replaces a list of old items with a list of new items.
        /// </summary>
        /// <param name="oldItemsRange">The old items range.</param>
        /// <param name="newItemsRange">The new items range.</param>
        /// <param name="newItemsToSkip">The new items to skip.</param>
        /// <param name="transaction">The transaction.</param>
        /// <remarks>
        /// TODO: newItemsToSkip is a HACK. Should find a better approach.
        /// </remarks>
        public void ReplaceRange(IEnumerable<TElement> oldItemsRange, IEnumerable<TElement> newItemsRange, List<int> newItemsToSkip, ITransaction transaction)
        {
            var oldItems = oldItemsRange.EnumerateSafely();
            var newItems = newItemsRange.EnumerateSafely();

            if (oldItems.Count == 0 && newItems.Count == 0)
            {
                return;
            }

            // Now begin replacing the items. It is safe to acquire a lock at this point as we won't be 
            // touching the source collection nor raising any events (yet).
            lock (BindableCollectionLock)
            {
                for (var relativeIndex = 0; relativeIndex < oldItems.Count || relativeIndex < newItems.Count; relativeIndex++)
                {
                    var oldItem = (relativeIndex < oldItems.Count) ? (object) oldItems[relativeIndex] : null;
                    var newItem = (relativeIndex < newItems.Count && !newItemsToSkip.Contains(relativeIndex)) ? (object) newItems[relativeIndex] : null;
                    var oldElement = (oldItem != null) ? (TElement) oldItem : default(TElement);
                    var newElement = (newItem != null) ? (TElement) newItem : default(TElement);

                    if (oldItem != null && newItem != null)
                    {
                        var oldItemIndex = IndexOf(oldElement);
                        if (oldItemIndex >= 0)
                        {
                            InnerList[oldItemIndex] = newElement;
                            transaction.LogReplaceEvent(oldElement, newElement, oldItemIndex);
                            _snapshotManager.Invalidate();
                        }
                        else
                        {
                            Add(newElement, transaction);
                        }
                    }
                    else if (newItem == null && oldItem == null) {}
                    else if (newItem == null)
                    {
                        Remove(oldElement, transaction);
                    }
                    else if (oldItem == null)
                    {
                        Add(newElement, transaction);
                    }
                }
            }
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// true if <paramref name="element"/> was successfully removed from the <see cref="T:BindableCollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:BindableCollection`1"/>.
        /// </returns>
        public bool Remove(TElement element)
        {
            return RemoveRange(new[] {element});
        }

        /// <summary>
        /// Removes the item at the specified index in the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            using (var transaction = BeginTransaction())
            {
                var item = default(TElement);
                lock (BindableCollectionLock)
                {
                    item = InnerList[index];
                    Remove(item, transaction);
                }
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:BindableCollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:BindableCollection`1"/>.
        /// </returns>
        public bool Remove(TElement element, ITransaction transaction)
        {
            return RemoveRange(new[] {element}, transaction);
        }

        /// <summary>
        /// Removes a range of items from the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="range">The items to remove.</param>
        public bool RemoveRange(IEnumerable<TElement> range)
        {
            using (var transaction = BeginTransaction())
            {
                return RemoveRange(range, transaction);
            }
        }

        /// <summary>
        /// Removes a range of items from the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="range">The items to remove.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public bool RemoveRange(IEnumerable<TElement> range, ITransaction transaction)
        {
            var result = false;
            var itemsToRemove = range.EnumerateSafely();

            lock (BindableCollectionLock)
            {
                foreach (var element in itemsToRemove)
                {
                    var index = IndexOf(element);
                    if (index >= 0)
                    {
                        InnerList.RemoveAt(index);
                        _snapshotManager.Invalidate();
                        transaction.LogRemoveEvent(element, index);
                        result = true;
                    }
                }
            }
            return result;
        }
        #endregion

        #region Clear
        /// <summary>
        /// Removes all items from the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        public void Clear()
        {
            using (var transaction = BeginTransaction())
            {
                Clear(transaction);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        public void Clear(ITransaction transaction)
        {
            lock (BindableCollectionLock)
            {
                InnerList.Clear();
                _snapshotManager.Invalidate();
                transaction.LogResetEvent();
            }
        }
        #endregion

        #region GetEnumerator
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="T:BindableCollection`1"/>. The 
        /// enumerator returned is a special kind of enumerator that allows the collection to be 
        /// modified even while it is being enumerated.
        /// </summary>
        /// <returns>
        /// An <see cref="T:IEnumerator`1"/> that can be used to iterate through the <see cref="T:BindableCollection`1"/> in a thread-safe way.
        /// </returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            lock (BindableCollectionLock)
            {
                return _snapshotManager.CreateEnumerator();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the <see cref="T:BindableCollection`1"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Rebuilds the snapshot.
        /// </summary>
        private List<TElement> RebuildSnapshotCallback()
        {
            lock (BindableCollectionLock)
            {
                var snapshot = new List<TElement>(InnerList.Count);
                snapshot.AddRange(InnerList);
                return snapshot;
            }
        }
        #endregion

        #region IBindableCollection<TElement> Members
        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        /// <remarks>Warning: No locks should be held when raising this event.</remarks>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>Warning: No locks should be held when raising this event.</remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:BindableCollection`1"/>.</returns>
        public int Count
        {
            get { return InnerList.Count; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {}
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
            using (var transaction = BeginTransaction())
            {
                lock (BindableCollectionLock)
                {
                    Add((TElement) value, transaction);
                    return IndexOf((TElement) value);
                }
            }
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
            return Contains((TElement) value);
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
            return IndexOf((TElement) value);
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
            Insert(index, (TElement) value);
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
            Remove((TElement) value);
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        /// <value></value>
        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (TElement) value; }
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
        /// Copies the elements of the <see cref="T:BindableCollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:BindableCollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
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
            var underlyingArray = (object[]) array;
            lock (BindableCollectionLock)
            {
                for (int arrayIndex = 0, innerIndex = index; innerIndex < InnerList.Count && arrayIndex < array.Length; innerIndex++, arrayIndex++)
                {
                    array.SetValue(InnerList[innerIndex], arrayIndex);
                }
            }
        }
        #endregion

        #region IList<TElement> Members
        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <value>The item at the specified index.</value>
        public TElement this[int index]
        {
            get
            {
                lock (BindableCollectionLock)
                {
                    return InnerList[index];
                }
            }
            set { Replace(InnerList[index], value); }
        }

        /// <summary>
        /// Determines whether the <see cref="T:BindableCollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:BindableCollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:BindableCollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(TElement item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:BindableCollection`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:BindableCollection`1"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(TElement item)
        {
            lock (BindableCollectionLock)
            {
                // List<T>.IndexOf(item) underneath uses object.Equals(). We want to use object.ReferenceEquals() so that 
                // overloaded Equals operations do not have an effect. 
                var index = -1;
                for (var i = 0; i < InnerList.Count; i++)
                {
                    if (_comparer.Equals(item, InnerList[i]))
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:BindableCollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:BindableCollection`1"/> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:BindableCollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:BindableCollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(TElement[] array, int arrayIndex)
        {
            lock (BindableCollectionLock)
            {
                InnerList.CopyTo(array, arrayIndex);
            }
        }
        #endregion

        /// <summary>
        /// Begins a transaction. Transactions can only be used by the thread controlling the 
        /// transaction, and involve locking the current collection so that other threads may 
        /// not access it. 
        /// </summary>
        public ITransaction BeginTransaction()
        {
            Monitor.Enter(BindableCollectionLock);
            var transaction = new Transaction(CommitTransaction);
            return transaction;
        }

        /// <summary>
        /// Commits the transaction. 
        /// </summary>
        /// <param name="transactionLog">The transaction log.</param>
        private void CommitTransaction(TransactionLog transactionLog)
        {
            Monitor.Exit(BindableCollectionLock);
            foreach (var eventToRaise in transactionLog.Events)
            {
                OnCollectionChanged(eventToRaise);
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "BindableCollection - Count: " + Count);
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
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
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}