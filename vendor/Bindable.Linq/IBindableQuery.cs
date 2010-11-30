namespace Bindable.Linq
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using Dependencies;

    /// <summary>
    /// This interface is supported by all Bindable LINQ result sets. As well as being an <see cref="T:IEnumerable"/>, 
    /// it provides a number of other properties and methods which take advantage of Bindable LINQ's data 
    /// binding and delayed execution features.
    /// </summary>
    public interface IBindableQuery : IBindableCollection, IAcceptsDependencies, IRefreshable, IConfigurable, ILoadable, IDisposable
    {
        /// <summary>
        /// Gets the number of items that are currently available in the result set.
        /// </summary>
        int CurrentCount { get; }
    }

    /// <summary>
    /// This interface is supported by all Bindable LINQ result sets. As well as being an <see cref="T:IEnumerable`1"/>, 
    /// it provides a number of other properties and methods which take advantage of Bindable LINQ's data 
    /// binding and delayed execution features.
    /// </summary>
    /// <typeparam name="TResult">The type of item being enumerated.</typeparam>
    public interface IBindableQuery<TResult> : IEnumerable<TResult>, IBindableCollection<TResult>, IBindableQuery
    {
        /// <summary>
        /// Gets the <see cref="T:TResult"/> at the specified index.
        /// </summary>
        /// <value></value>
        TResult this[int index] { get; }
    }
}