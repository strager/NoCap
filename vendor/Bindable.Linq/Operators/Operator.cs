namespace Bindable.Linq.Operators
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Configuration;
    using Dependencies;

    /// <summary>
    /// Serves as a base class for all operator functions. From Bindable LINQ's perspective,
    /// an <see cref="T:Operator`2"/> is a LINQ operation which tranforms a single source items
    /// into single result item. This makes it different to an <see cref="T:Iterator`2"/> which
    /// transforms a collection into another collection, or an <see cref="T:Aggregator`2"/>
    /// which transforms a collection into a single element.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class Operator<TSource, TResult> : IBindable<TResult>, IConfigurable, IAcceptsDependencies
    {
        private static readonly PropertyChangedEventArgs CurrentPropertyChangedEventArgs = new PropertyChangedEventArgs("Current");
        private readonly List<IDependency> _dependencies;
        private readonly EventHandler<PropertyChangedEventArgs> _eventHandler;
        private readonly object _operatorLock = new object();
        private readonly IBindable<TSource> _source;
        private readonly PropertyChangeObserver _sourcePropertyChangeObserver;
        private TResult _current;
        private bool _isSourceLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="Operator&lt;TSource, TResult&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public Operator(IBindable<TSource> source)
        {
            _dependencies = new List<IDependency>();
            _eventHandler = Source_PropertyChanged;
            _sourcePropertyChangeObserver = new PropertyChangeObserver(_eventHandler);
            _source = source;
            _sourcePropertyChangeObserver.Attach(_source);
        }

        /// <summary>
        /// Gets the operator lock.
        /// </summary>
        /// <value>The operator lock.</value>
        protected object OperatorLock
        {
            get { return _operatorLock; }
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        public IBindable<TSource> Source
        {
            get { return _source; }
        }

        #region IAcceptsDependencies Members
        /// <summary>
        /// Sets a new dependency on a Bindable LINQ operation.
        /// </summary>
        /// <param name="definition">A definition of the dependency.</param>
        public void AcceptDependency(IDependencyDefinition definition)
        {
            if (definition.AppliesToSingleElement())
            {
                var dependency = definition.ConstructForElement(_source, Configuration.CreatePathNavigator());
                dependency.SetReevaluateCallback(o => Refresh());
                _dependencies.Add(dependency);
            }
        }
        #endregion

        #region IBindable<TResult> Members
        /// <summary>
        /// The resulting value. Rather than being returned directly, the value is housed
        /// within the <see cref="T:IBindable`1"/> container so that it can be updated when
        /// the source it was created from changes.
        /// </summary>
        /// <value></value>
        public TResult Current
        {
            get
            {
                EnsureLoaded();
                return _current;
            }
            set
            {
                _current = value;
                OnPropertyChanged(CurrentPropertyChangedEventArgs);
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Refreshes the object.
        /// </summary>
        public void Refresh()
        {
            RefreshOverride();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _sourcePropertyChangeObserver.Dispose();
            foreach (var dependency in _dependencies)
            {
                dependency.Dispose();
            }
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
                if (Source is IConfigurable)
                {
                    result = ((IConfigurable) Source).Configuration;
                }
                return result;
            }
        }
        #endregion

        private void EnsureLoaded()
        {
            var refreshNeeded = false;

            lock (OperatorLock)
            {
                if (_isSourceLoaded == false)
                {
                    _isSourceLoaded = true;
                    refreshNeeded = true;
                }
            }

            if (refreshNeeded)
            {
                Refresh();
            }
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// When overridden in a derived class, refreshes the object.
        /// </summary>
        protected abstract void RefreshOverride();

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
    }
}