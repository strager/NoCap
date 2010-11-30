namespace Bindable.Linq.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Configuration;
    using Dependencies;
    using Helpers;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    internal sealed class BindableCollectionInterceptor<TElement> : IBindableCollectionInterceptor<TElement>, IBindableQuery<TElement>
    {
        private readonly IBindableCollection<TElement> _inner;
        private readonly EventHandler<NotifyCollectionChangedEventArgs> _inner_CollectionChanged;
        private readonly WeakEventReference<NotifyCollectionChangedEventArgs> _inner_CollectionChangedWeak;
        private readonly EventHandler<PropertyChangedEventArgs> _inner_PropertyChanged;
        private readonly WeakEventReference<PropertyChangedEventArgs> _inner_PropertyChangedWeak;
        private readonly List<Action<TElement>> _postYieldSteps;
        private readonly List<Action<TElement>> _preYieldSteps;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindableCollectionInterceptor&lt;TElement&gt;"/> class.
        /// </summary>
        /// <param name="inner">The inner.</param>
        public BindableCollectionInterceptor(IBindableCollection<TElement> inner)
        {
            _inner = inner;
            _preYieldSteps = new List<Action<TElement>>();
            _postYieldSteps = new List<Action<TElement>>();
            _inner_CollectionChanged = Inner_CollectionChanged;
            _inner_PropertyChanged = Inner_PropertyChanged;
            _inner_CollectionChangedWeak = new WeakEventReference<NotifyCollectionChangedEventArgs>(_inner_CollectionChanged);
            _inner_PropertyChangedWeak = new WeakEventReference<PropertyChangedEventArgs>(_inner_PropertyChanged);
            _inner.CollectionChanged += _inner_CollectionChangedWeak.WeakEventHandler;
            _inner.PropertyChanged += _inner_PropertyChangedWeak.WeakEventHandler;
        }

        #region IBindableCollectionInterceptor<TElement> Members
        /// <summary>
        /// Gets the count of items in the collection.
        /// </summary>
        public int Count
        {
            get { return _inner.Count; }
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Adds the pre yield step.
        /// </summary>
        /// <param name="step">The step.</param>
        public void AddPreYieldStep(Action<TElement> step)
        {
            _preYieldSteps.Add(step);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            foreach (var element in _inner)
            {
                foreach (var action in _preYieldSteps)
                {
                    action(element);
                }
                yield return element;
                foreach (var action in _postYieldSteps)
                {
                    action(element);
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _inner.CollectionChanged -= _inner_CollectionChangedWeak.WeakEventHandler;
            _inner.PropertyChanged -= _inner_PropertyChangedWeak.WeakEventHandler;
        }
        #endregion

        #region IBindableQuery<TElement> Members
        public TElement this[int index]
        {
            get
            {
                var result = default(TElement);
                var bindable = _inner as IBindableQuery<TElement>;
                if (bindable != null)
                {
                    result = bindable[index];
                }
                return result;
            }
        }

        public int CurrentCount
        {
            get
            {
                var result = 0;
                var bindable = _inner as IBindableQuery<TElement>;
                if (bindable != null)
                {
                    result = bindable.CurrentCount;
                }
                else
                {
                    result = Count;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IBindingConfiguration Configuration
        {
            get
            {
                var result = BindingConfigurations.Default;
                if (_inner is IConfigurable)
                {
                    result = ((IConfigurable) _inner).Configuration;
                }
                return result;
            }
        }

        public void AcceptDependency(IDependencyDefinition definition) {}

        public void Refresh()
        {
            var refreshable = _inner as IRefreshable;
            if (refreshable != null)
            {
                refreshable.Refresh();
            }
            else
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool IsLoading
        {
            get
            {
                var result = false;
                var loadable = _inner as ILoadable;
                if (loadable != null)
                {
                    result = loadable.IsLoading;
                }
                return result;
            }
        }
        #endregion

        /// <summary>
        /// Adds the post yield step.
        /// </summary>
        /// <param name="step">The step.</param>
        public void AddPostYieldStep(Action<TElement> step)
        {
            _postYieldSteps.Add(step);
        }

        private void Inner_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        private void Inner_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}