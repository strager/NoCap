namespace Bindable.Linq.Collections
{
    using System;

    /// <summary>
    /// Implemented by bindable collections that allow clients to be notified before an item is yielded.
    /// </summary>
    public interface IBindableCollectionInterceptor : IBindableCollection {}

    /// <summary>
    /// Implemented by bindable collections that allow clients to be notified before an item is yielded.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    public interface IBindableCollectionInterceptor<TElement> : IBindableCollection<TElement>, IBindableCollectionInterceptor
    {
        /// <summary>
        /// Adds the pre yield step.
        /// </summary>
        /// <param name="step">The step.</param>
        void AddPreYieldStep(Action<TElement> step);
    }
}