namespace Bindable.Linq.Dependencies
{
    using System;
    using System.Collections.Specialized;
    using Helpers;

    /// <summary>
    /// Manages the subscription of CollectionChanged events on items.
    /// </summary>
    internal sealed class CollectionChangeObserver : EventDependency<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>
    {
        private readonly WeakEventReference<NotifyCollectionChangedEventArgs> _weakEvent;
        private NotifyCollectionChangedEventHandler _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionChangeObserver"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public CollectionChangeObserver(EventHandler<NotifyCollectionChangedEventArgs> callback)
        {
            _weakEvent = new WeakEventReference<NotifyCollectionChangedEventArgs>(callback);
            _callback = new NotifyCollectionChangedEventHandler(callback);
        }

        /// <summary>
        /// When overriden in a derived class, allows the class to subscribe a given event handler to
        /// the publishing class.
        /// </summary>
        /// <param name="publisher">The event publisher.</param>
        protected override void AttachOverride(INotifyCollectionChanged publisher)
        {
            publisher.CollectionChanged += _weakEvent.WeakEventHandler;
        }

        /// <summary>
        /// When overriden in a derived class, allows the class to unsubscribe a given event handler from
        /// the publishing class.
        /// </summary>
        /// <param name="publisher">The event publisher.</param>
        protected override void DetachOverride(INotifyCollectionChanged publisher)
        {
            publisher.CollectionChanged -= _weakEvent.WeakEventHandler;
        }

        /// <summary>
        /// When overridden in a derived class, allows the class to add custom code when the object is disposed.
        /// </summary>
        protected override void DisposeOverride() {}
    }
}