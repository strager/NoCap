namespace Bindable.Linq.Helpers
{
    using System;
    using System.Threading;

    internal sealed class WeakTimer : IDisposable
    {
        private readonly WeakReference _callbackReference;
        private readonly TimeSpan _pollTime;
        private Timer _timer;

        public WeakTimer(TimeSpan pollTime, Action callback)
        {
            _pollTime = pollTime;
            _callbackReference = new WeakReference(callback, true);
        }

        #region IDisposable Members
        public void Dispose()
        {
            Pause();
            _timer.Dispose();
        }
        #endregion

        public void Start()
        {
            _timer = new Timer(TimerTickCallback, null, _pollTime, _pollTime);
        }

        public void Pause()
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void Continue()
        {
            if (_timer != null)
            {
                _timer.Change(_pollTime, _pollTime);
            }
        }

        private void TimerTickCallback(object o)
        {
            var action = _callbackReference.Target as Action;
            if (action != null)
            {
                action();
            }
        }
    }
}