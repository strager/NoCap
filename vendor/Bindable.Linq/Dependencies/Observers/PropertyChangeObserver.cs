namespace Bindable.Linq.Dependencies
{
    using System;
    using System.ComponentModel;
    using Helpers;

    /// <summary>
    /// Manages the subscription of PropertyChanged events on items.
    /// </summary>
    internal sealed class PropertyChangeObserver : EventDependency<INotifyPropertyChanged, PropertyChangedEventArgs>
    {
        private readonly EventHandler<PropertyChangedEventArgs> _callback;
        private readonly WeakEventReference<PropertyChangedEventArgs> _weakHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangeObserver"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public PropertyChangeObserver(EventHandler<PropertyChangedEventArgs> callback)
        {
            _callback = callback;
            _weakHandler = new WeakEventReference<PropertyChangedEventArgs>(callback);
        }

        /// <summary>
        /// When overriden in a derived class, allows the class to subscribe a given event handler to
        /// the publishing class.
        /// </summary>
        /// <param name="publisher">The event publisher.</param>
        protected override void AttachOverride(INotifyPropertyChanged publisher)
        {
            publisher.PropertyChanged += _weakHandler.WeakEventHandler;
        }

        /// <summary>
        /// When overriden in a derived class, allows the class to unsubscribe a given event handler from
        /// the publishing class.
        /// </summary>
        /// <param name="publisher">The event publisher.</param>
        protected override void DetachOverride(INotifyPropertyChanged publisher)
        {
            publisher.PropertyChanged -= _weakHandler.WeakEventHandler;
        }

        /// <summary>
        /// When overridden in a derived class, allows the class to add custom code when the object is disposed.
        /// </summary>
        protected override void DisposeOverride()
        {
            if (_weakHandler != null)
            {
                _weakHandler.Dispose();
            }
        }
    }
}