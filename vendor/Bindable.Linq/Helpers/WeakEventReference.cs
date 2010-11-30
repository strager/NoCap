namespace Bindable.Linq.Helpers
{
    using System;

    /// <summary>
    /// An event handler wrapper used to create weak-reference event handlers, so that event subscribers 
    /// can be garbage collected without the event publisher interfering. 
    /// </summary>
    /// <typeparam name="A">The type of event arguments used in the event handler.</typeparam>
    /// <remarks>
    /// To understand why this class is needed, see this page: 
    ///     http://www.paulstovell.net/blog/index.php/wpf-binding-bug-leads-to-possible-memory-issues/
    /// For examples on how this is used, it is best to look at the unit test: 
    ///     WeakEventReferenceTests.cs
    /// </remarks>
    internal sealed class WeakEventReference<A> : IDisposable
        where A : EventArgs
    {
        private readonly WeakReference _callbackReference;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakEventReference&lt;A&gt;"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public WeakEventReference(EventHandler<A> callback)
        {
            _callbackReference = new WeakReference(callback, true);
        }

        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// Used as the event handler which should be subscribed to source collections.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WeakEventHandler(object sender, A e)
        {
            lock (_lock)
            {
                var callback = _callbackReference.Target as EventHandler<A>;
                if (callback != null)
                {
                    callback(sender, e);
                }
            }
        }
    }
}