namespace Bindable.Linq.Helpers
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using Collections;

    /// <summary>
    /// Performs actions on the elements of a collection when they are added or removed. Ensures 
    /// the action is always performed at least once. 
    /// </summary>
    /// <remarks>
    /// This object uses a direct event reference rather than weak references on purpose. The lifetime of 
    /// the object should be coupled to the owning class. Use the <see cref="Dispose"/> method to 
    /// unhook the event handler.
    /// </remarks>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    internal sealed class ElementActioner<TElement> : IDisposable
    {
        private readonly Action<TElement> _addAction;
        private readonly IBindableCollection<TElement> _collection;
        private readonly BindableCollection<TElement> _copy;
        private readonly EventHandler<NotifyCollectionChangedEventArgs> _eventHandler;
        private readonly object _object = new object();
        private readonly Action<TElement> _removeAction;
        private readonly WeakEventReference<NotifyCollectionChangedEventArgs> _weakEventHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementActioner&lt;TElement&gt;"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="addAction">The add action.</param>
        /// <param name="removeAction">The remove action.</param>
        public ElementActioner(IBindableCollection<TElement> collection, Action<TElement> addAction, Action<TElement> removeAction)
        {
            _addAction = addAction;
            _removeAction = removeAction;
            _collection = collection;

            _copy = new BindableCollection<TElement>();
            _eventHandler = Collection_CollectionChanged;
            _weakEventHandler = new WeakEventReference<NotifyCollectionChangedEventArgs>(_eventHandler);
            _collection.CollectionChanged += _weakEventHandler.WeakEventHandler;

            var interceptor = collection as IBindableCollectionInterceptor<TElement>;
            if (interceptor != null)
            {
                interceptor.AddPreYieldStep(element =>
                {
                    HandleElement(NotifyCollectionChangedAction.Add, element);
                    _copy.Add(element);
                });
            }
            else
            {
                _collection.ForEach(element =>
                {
                    HandleElement(NotifyCollectionChangedAction.Add, element);
                    _copy.Add(element);
                });
            }
        }

        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_collection != null)
            {
                _copy.ForEach(e => HandleElement(NotifyCollectionChangedAction.Remove, e));
                _collection.CollectionChanged -= _weakEventHandler.WeakEventHandler;
                _weakEventHandler.Dispose();
            }
        }
        #endregion

        private void HandleElement(NotifyCollectionChangedAction action, TElement element)
        {
            if (action == NotifyCollectionChangedAction.Add)
            {
                _addAction(element);
            }
            else if (action == NotifyCollectionChangedAction.Remove)
            {
                _removeAction(element);
            }
        }

        /// <summary>
        /// Handles the CollectionChanged event of the Collection.
        /// </summary>
        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TElement element in e.NewItems)
                    {
                        HandleElement(NotifyCollectionChangedAction.Add, element);
                        _copy.Add(element);
                    }
                    break;
#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
                    break;
#endif
                case NotifyCollectionChangedAction.Remove:
                    foreach (TElement element in e.OldItems)
                    {
                        HandleElement(NotifyCollectionChangedAction.Remove, element);
                        _copy.Remove(element);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (TElement element in e.OldItems)
                    {
                        HandleElement(NotifyCollectionChangedAction.Remove, element);
                    }
                    foreach (TElement element in e.NewItems)
                    {
                        HandleElement(NotifyCollectionChangedAction.Add, element);
                    }
                    _copy.ReplaceRange(e.OldItems.Cast<TElement>(), e.NewItems.Cast<TElement>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    HandleReset();
                    break;
            }
        }

        private void HandleReset()
        {
            _copy.ForEach(a => HandleElement(NotifyCollectionChangedAction.Remove, a));
            _collection.ForEach(a => HandleElement(NotifyCollectionChangedAction.Add, a));
            _copy.Clear();
            _copy.AddRange(_collection);
        }
    }
}