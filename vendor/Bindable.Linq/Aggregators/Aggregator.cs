namespace Bindable.Linq.Aggregators
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Configuration;
    using Dependencies;
    using Helpers;
    using Iterators;

    /// <summary>
    /// Serves as a base class for all aggregate functions. From Bindable LINQ's perspective,
    /// an <see cref="Aggregator{TSource,TResult}"/> is a LINQ operation which tranforms a collection of items
    /// into an item. This makes it different to an <see cref="Iterator{TSource,TResult}"/> which
    /// transforms a collection into another collection, or an <see cref="T:Operator`2"/>
    /// which transforms one item into another.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class Aggregator<TSource, TResult> : IBindable<TResult>, IConfigurable, IDisposable
    {
        private static readonly PropertyChangedEventArgs CurrentPropertyChangedEventArgs = new PropertyChangedEventArgs("Current");
        private readonly object _aggregatorLock = new object();
        private readonly StateScope _calculatingState;
        private readonly CollectionChangeObserver _collectionChangedObserver;
        private readonly IBindableCollection<TSource> _sourceCollection;
        private bool _isSourceCollectionLoaded;
        private TResult _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aggregator&lt;TSource, TResult&gt;"/> class.
        /// </summary>
        /// <param name="sourceCollection">The source collection.</param>
        protected Aggregator(IBindableCollection<TSource> sourceCollection)
        {
            _collectionChangedObserver = new CollectionChangeObserver(SourceCollection_CollectionChanged);
            _calculatingState = new StateScope();
            _sourceCollection = sourceCollection;
            _collectionChangedObserver.Attach(sourceCollection);
        }

        /// <summary>
        /// Gets the lock object on this aggregate.
        /// </summary>
        protected object AggregateLock
        {
            get { return _aggregatorLock; }
        }

        /// <summary>
        /// Gets a state scope that can be entered to indicate the aggregate is being calculated.
        /// </summary>
        protected StateScope CalculatingState
        {
            get { return _calculatingState; }
        }

        /// <summary>
        /// Gets the source collections that this aggregate is aggregating.
        /// </summary>
        protected IBindableCollection<TSource> SourceCollection
        {
            get { return _sourceCollection; }
        }

        #region IBindable<TResult> Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The resulting value. Rather than being returned directly, the value is housed
        /// within the <see cref="T:IBindableElement`1"/> container so that it can be updated when
        /// the source it was created from changes.
        /// </summary>
        /// <value></value>
        public TResult Current
        {
            get
            {
                EnsureLoaded();
                return _value;
            }
            protected set
            {
                var valueChanged = false;
                lock (AggregateLock)
                {
                    if (!Equals(_value, value))
                    {
                        _value = value;
                        valueChanged = true;
                    }
                }
                if (valueChanged)
                {
                    OnPropertyChanged(CurrentPropertyChangedEventArgs);
                }
            }
        }

        /// <summary>
        /// Refreshes the value by forcing it to be recalculated or reconsidered.
        /// </summary>
        public void Refresh()
        {
            Aggregate();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _calculatingState.Leave();
            GC.SuppressFinalize(this);
        }
        #endregion

        #region IConfigurable Members
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IBindingConfiguration Configuration
        {
            get
            {
                var result = BindingConfigurations.Default;
                if (SourceCollection is IConfigurable)
                {
                    result = ((IConfigurable) SourceCollection).Configuration;
                }
                return result;
            }
        }
        #endregion

        /// <summary>
        /// When overridden in a derived class, provides the aggregator the opportunity to calculate the 
        /// value.
        /// </summary>
        protected abstract void AggregateOverride();

        private void Aggregate()
        {
            using (CalculatingState.Enter())
            {
                AggregateOverride();
            }
        }

        private void EnsureLoaded()
        {
            var calculationNeeded = false;

            lock (AggregateLock)
            {
                if (_isSourceCollectionLoaded == false)
                {
                    _isSourceCollectionLoaded = true;
                    calculationNeeded = true;
                }
            }

            if (calculationNeeded)
            {
                Aggregate();
            }
        }

        private void SourceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}