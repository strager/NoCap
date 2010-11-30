namespace Bindable.Linq
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// This interface is implemented by the results of any Bindable LINQ query which result in 
    /// single items, as opposed to collections. 
    /// </summary>
    public interface IBindable : INotifyPropertyChanged, IRefreshable, IDisposable {}

    /// <summary>
    /// This interface is implemented by the results of any Bindable LINQ query which result in 
    /// single items, as opposed to collections. 
    /// </summary>
    /// <typeparam name="TValue">The type of the value contained within the instance.</typeparam>
    public interface IBindable<TValue> : IBindable
    {
        /// <summary>
        /// The resulting value. Rather than being returned directly, the value is housed 
        /// within the <see cref="T:IBindable`1"/> container so that it can be updated when 
        /// the source it was created from changes. 
        /// </summary>
        TValue Current { get; }
    }
}