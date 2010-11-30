namespace Bindable.Linq.Transactions
{
    using System;

    /// <summary>
    /// An interface implemented by classes that record collection changed events and package them up, before they are 
    /// raised. When one class is recording a transaction, others will wait before beginning 
    /// a transaction.
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Records the fact that an element was added. When the transaction is completed, an 
        /// Add event will be raised. 
        /// </summary>
        /// <param name="elementAdded">The element added.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="elementAdded"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
        void LogAddEvent(object elementAdded, int index);

        /// <summary>
        /// Records the fact that an element was moved. When the transaction is completed, a 
        /// Move event will be raised. 
        /// </summary>
        /// <param name="elementMoved">The element moved.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="elementMoved"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="oldIndex"/> is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newIndex"/> is less than zero.</exception>
        void LogMoveEvent(object elementMoved, int oldIndex, int newIndex);

        /// <summary>
        /// Records the fact that an element was removed. When the transaction is completed, a 
        /// Remove event will be raised. 
        /// </summary>
        /// <param name="removedElement">The removed element.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="removedElement"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
        void LogRemoveEvent(object removedElement, int index);

        /// <summary>
        /// Records the fact that an element was replaced. When the transaction is completed, a 
        /// Replace event will be raised. 
        /// </summary>
        /// <param name="originalElement">The original element.</param>
        /// <param name="replacementElement">The replacement element.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="originalElement"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="replacementElement"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
        void LogReplaceEvent(object originalElement, object replacementElement, int index);

        /// <summary>
        /// Records the fact that the collection has changed dramatically and should be refreshed.  When the transaction is completed, a 
        /// Reset event will be raised. 
        /// </summary>
        void LogResetEvent();

        /// <summary>
        /// Leaves the transaction, relenquishes any locks, and raises any events logged during the transaction. 
        /// </summary>
        void Commit();
    }
}