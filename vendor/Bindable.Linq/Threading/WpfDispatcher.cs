namespace Bindable.Linq.Threading
{
    using System;
    using System.Windows.Threading;

#if !SILVERLIGHT
    /// <summary>
    /// This dispatcher is used at runtime by both Windows Forms and WPF. The WPF Dispatcher 
    /// class works within Windows Forms, so this appears to be safe.
    /// </summary>
    public class WpfDispatcher : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfDispatcher"/> class.
        /// </summary>
        public WpfDispatcher()
            : this(Dispatcher.CurrentDispatcher) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfDispatcher"/> class.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        public WpfDispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        #region IDispatcher Members
        /// <summary>
        /// Dispatches the specified action to the thread.
        /// </summary>
        /// <param name="actionToInvoke">The action to invoke.</param>
        public void Invoke(Action actionToInvoke)
        {
            _dispatcher.Invoke(DispatcherPriority.DataBind, actionToInvoke);
        }
        #endregion
    }
#endif
}