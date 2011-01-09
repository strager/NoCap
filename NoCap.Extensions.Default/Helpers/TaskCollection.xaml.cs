using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using Bindable.Linq;
using Bindable.Linq.Collections;
using NoCap.Library.Progress;
using NoCap.Library.Tasks;

namespace NoCap.Extensions.Default.Helpers {
    public sealed class TaskCollection : IBindableCollection<ICommandTask> {
        private readonly BindableCollection<ICommandTask> collection;
        private readonly Dispatcher collectionDispatcher;

        private readonly object syncRoot = new object();

        public TaskCollection() {
            this.collection = new BindableCollection<ICommandTask>();
            this.collectionDispatcher = Dispatcher.CurrentDispatcher;

            this.collection.CollectionChanged += (sender, e) => OnCollectionChanged(e);
        }

        private void AttachTaskHandlers(ICommandTask task) {
            task.ProgressTracker.ProgressUpdated += TaskProgressUpdated;
        }

        private void DetachTaskHandlers(ICommandTask task) {
            task.ProgressTracker.ProgressUpdated -= TaskProgressUpdated;
        }

        private void TaskProgressUpdated(object sender, ProgressUpdatedEventArgs e) {
            OnProgressUpdated(new EventArgs());
        }

        public void AddTask(ICommandTask task) {
            if (!this.collectionDispatcher.CheckAccess()) {
                this.collectionDispatcher.BeginInvoke(new Action<ICommandTask>(AddTask), task);

                return;
            }

            lock (this.syncRoot) {
                this.collection.Add(task);
            }

            AttachTaskHandlers(task);
            OnProgressUpdated(new EventArgs());
        }

        public void RemoveTask(ICommandTask task) {
            if (!this.collectionDispatcher.CheckAccess()) {
                this.collectionDispatcher.BeginInvoke(new Action<ICommandTask>(RemoveTask), task);

                return;
            }

            lock (this.syncRoot) {
                this.collection.Remove(task);
            }

            DetachTaskHandlers(task);
            OnProgressUpdated(new EventArgs());
        }

        public void RemoveFinishedTasks() {
            if (!this.collectionDispatcher.CheckAccess()) {
                this.collectionDispatcher.BeginInvoke(new Action(RemoveFinishedTasks));

                return;
            }

            ICommandTask[] finished;

            lock (this.syncRoot) {
                // Convert to array to prevent modifying the collection while reading from it
                finished = this.collection.Where((task) => task.State == TaskState.Canceled || task.State == TaskState.Completed).ToArray();

                // Weird bug prevents us from using a transaction or RemoveRange.
                // Binding.Linq extension methds don't seem to support a "remove range" operation,
                // which transactions and RemoveRange give.  The following fires many
                // plain Remove events.  While inefficient, it doesn't crash.
                // TODO Ask for support in Bindable.Linq.
                foreach (var task in finished) {
                    this.collection.Remove(task);
                }
            }

            foreach (var task in finished) {
                DetachTaskHandlers(task);
            }

            OnProgressUpdated(new EventArgs());
        }

        public IEnumerator<ICommandTask> GetEnumerator() {
            // Be careful.

            IEnumerator<ICommandTask> enumerator = null;

            try {
                lock (this.syncRoot) {
                    enumerator = this.collection.GetEnumerator();
                }

                while (true) {
                    ICommandTask task;

                    lock (this.syncRoot) {
                        if (!enumerator.MoveNext()) {
                            break;
                        }

                        task = enumerator.Current;
                    }

                    yield return task;
                }
            } finally {
                if (enumerator != null) {
                    enumerator.Dispose();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            var handler = CollectionChanged;

            if (handler != null) {
                handler(this, e);
            }
        }

        private PropertyChangedEventHandler propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
            add    { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }

        public void Notify(string propertyName) {
            var handler = this.propertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        void IDisposable.Dispose() {
        }

        public int Count {
            get {
                lock (this.syncRoot) {
                    return this.collection.Count;
                }
            }
        }

        public double Progress {
            get {
                lock (this.syncRoot) {
                    return this.collection.Average((task) => task.ProgressTracker.Progress).Current;
                }
            }
        }

        public event EventHandler ProgressUpdated;

        private void OnProgressUpdated(EventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }
    }
}