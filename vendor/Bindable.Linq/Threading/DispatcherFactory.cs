namespace Bindable.Linq.Threading
{
    using System.Windows.Threading;

    internal sealed class DispatcherFactory
    {
        public static IDispatcher Create(Dispatcher dispatcher)
        {
            IDispatcher result = null;
#if SILVERLIGHT
            result = new SilverlightDispatcher(dispatcher);
#else
            result = new WpfDispatcher(dispatcher);
#endif
            return result;
        }
    }
}