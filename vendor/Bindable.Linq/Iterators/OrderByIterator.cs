namespace Bindable.Linq.Iterators
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The Iterator created when ordering a collection.
    /// </summary>
    /// <typeparam name="TElement">The source collection type.</typeparam>
    /// <typeparam name="TKey">The type of key used to determine which properties to sort by.</typeparam>
    internal sealed class OrderByIterator<TElement, TKey> : Iterator<TElement, TElement>, IOrderedBindableQuery<TElement>
        where TElement : class
    {
        private readonly ItemSorter<TElement, TKey> _itemSorter;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByIterator&lt;S, K&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="itemSorter">The item sorter.</param>
        public OrderByIterator(IBindableCollection<TElement> source, ItemSorter<TElement, TKey> itemSorter)
            : base(source)
        {
            _itemSorter = itemSorter;
        }

        #region IOrderedBindableQuery<TElement> Members
        /// <summary>
        /// Performs a subsequent ordering on the elements of an <see cref="T:IOrderedIterator[TElement]"/>
        /// according to a key.
        /// </summary>
        /// <typeparam name="TNewKey">The type of the key.</typeparam>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        /// <returns></returns>
        public IOrderedBindableQuery<TElement> CreateOrderedIterator<TNewKey>(Func<TElement, TNewKey> keySelector, IComparer<TNewKey> comparer, bool descending)
        {
            return new OrderByIterator<TElement, TNewKey>(SourceCollection, new ItemSorter<TElement, TNewKey>(_itemSorter, keySelector, comparer, !descending));
        }
        #endregion

        /// <summary>
        /// When implemented in a derived class, processes all items in the <see cref="P:SourceCollection"/>.
        /// </summary>
        protected override void LoadSourceCollection()
        {
            ReactToAddRange(0, SourceCollection);
        }

        /// <summary>
        /// Compares the specified items.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns></returns>
        public int Compare(TElement lhs, TElement rhs)
        {
            return _itemSorter.Compare(lhs, rhs);
        }

        /// <summary>
        /// When overridden in a derived class, processes an Add event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="addedItems">The added items.</param>
        protected override void ReactToAddRange(int sourceStartingIndex, IEnumerable<TElement> addedItems)
        {
            ResultCollection.InsertRangeOrder(addedItems, Compare);
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
            ResultCollection.RemoveRange(removedItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a Replace event over a range of items.
        /// </summary>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newItems">The new items.</param>
        protected override void ReactToReplaceRange(IEnumerable<TElement> oldItems, IEnumerable<TElement> newItems)
        {
            ReactToRemoveRange(oldItems);
            ReactToAddRange(-1, newItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a PropertyChanged event on a source item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected override void ReactToItemPropertyChanged(TElement item, string propertyName)
        {
            ResultCollection.MoveItemOrdered(item, Compare);
        }
    }
}