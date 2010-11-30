namespace Bindable.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// An interface implemented by all Bindable LINQ bindable collections.
    /// </summary>
    public interface IBindableCollection : IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets the count of items in the collection.
        /// </summary>
        int Count { get; }
    }

    /// <summary>
    /// An interface implemented by all Bindable LINQ bindable collections.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    public interface IBindableCollection<TElement> : IEnumerable<TElement>, IBindableCollection {}
}