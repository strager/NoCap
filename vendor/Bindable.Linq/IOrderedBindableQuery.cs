namespace Bindable.Linq
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface is supported by all Bindable LINQ result sets. As well as being an <see cref="T:IEnumerable`1"/>, 
    /// it provides a number of other properties and methods which take advantage of Bindable LINQ's data 
    /// binding and delayed execution features. It also allows further sorting of the items through the 
    /// <see cref="M:CreateOrderedIterator`1"/> method.
    /// </summary>
    /// <typeparam name="TResult">The type of item being enumerated.</typeparam>
    public interface IOrderedBindableQuery<TResult> : IBindableQuery<TResult>
        where TResult : class
    {
        /// <summary>
        /// Performs a subsequent ordering on the elements of an 
        /// <see cref="T:IOrderedSyncLinqCollection`1" /> according to a key.
        /// </summary>
        /// <param name="keySelector">The <see cref="T:System.Func`2" /> used to extract the key for each element.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> used to compare keys for placement in the returned sequence.</param>
        /// <param name="descending">true to sort the elements in descending order; false to sort the elements in ascending order.</param>
        /// <typeparam name="TKey">The type of the key produced by <paramref name="keySelector" />.</typeparam>
        /// <returns>
        /// An <see cref="T:IOrderedSyncLinqCollection`1" /> whose elements are sorted according to a key.
        /// </returns>
        IOrderedBindableQuery<TResult> CreateOrderedIterator<TKey>(Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }
}