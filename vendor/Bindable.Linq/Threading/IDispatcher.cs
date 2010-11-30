namespace Bindable.Linq.Threading
{
    using System;

    /// <summary>
    /// Provides a wrapper around the process of dispatching actions onto 
    /// different threads, primarily for unit testing.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Dispatches the specified action to the thread.
        /// </summary>
        /// <param name="actionToInvoke">The action to invoke.</param>
        void Invoke(Action actionToInvoke);
    }
}