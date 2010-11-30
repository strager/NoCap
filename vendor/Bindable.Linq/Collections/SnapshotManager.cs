namespace Bindable.Linq.Collections
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Manages the snapshots of a collection.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    internal sealed class SnapshotManager<TElement>
    {
        private readonly Func<List<TElement>> _rebuildCallback;
        private readonly object _snapshotManagerLock = new object();
        private List<TElement> _latestSnapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotManager&lt;TElement&gt;"/> class.
        /// </summary>
        /// <param name="rebuildCallback">The rebuild callback.</param>
        public SnapshotManager(Func<List<TElement>> rebuildCallback)
        {
            _rebuildCallback = rebuildCallback;
        }

        /// <summary>
        /// Invalidates this instance.
        /// </summary>
        public void Invalidate()
        {
            lock (_snapshotManagerLock)
            {
                _latestSnapshot = null;
            }
        }

        /// <summary>
        /// Creates the enumerator.
        /// </summary>
        public IEnumerator<TElement> CreateEnumerator()
        {
            lock (_snapshotManagerLock)
            {
                if (_latestSnapshot == null)
                {
                    _latestSnapshot = _rebuildCallback();
                }
                return _latestSnapshot.GetEnumerator();
            }
        }
    }
}