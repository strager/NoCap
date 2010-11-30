namespace Bindable.Linq.Transactions
{
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    /// The transaction log provides a record of all of the events that should be
    /// raised once the transaction has completed.
    /// </summary>
    public sealed class TransactionLog
    {
        private readonly IEnumerable<NotifyCollectionChangedEventArgs> _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLog"/> class.
        /// </summary>
        /// <param name="events">The events.</param>
        public TransactionLog(IEnumerable<NotifyCollectionChangedEventArgs> events)
        {
            _events = events;
        }

        /// <summary>
        /// Gets the events to raise.
        /// </summary>
        public IEnumerable<NotifyCollectionChangedEventArgs> Events
        {
            get { return _events; }
        }
    }
}