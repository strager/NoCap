using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace NoCap.Extensions.Default.Helpers {
    internal class StoryboardQueue {
        private readonly Queue<StoryboardQueueItem> queue = new Queue<StoryboardQueueItem>();
        private Storyboard runningStoryboard = null;

        public void Enqueue(Storyboard storyboard, FrameworkElement element) {
            Enqueue(storyboard, element, true);
        }

        public void Enqueue(Storyboard storyboard, FrameworkElement element, Action completedCallback) {
            Enqueue(storyboard, element, completedCallback, true);
        }

        public void Enqueue(Storyboard storyboard, FrameworkElement element, bool allowSame) {
            Enqueue(storyboard, element, null, allowSame);
        }

        public void Enqueue(Storyboard storyboard, FrameworkElement element, Action completedCallback, bool allowSame) {
            if (!allowSame && this.runningStoryboard == storyboard) {
                return;
            }

            queue.Enqueue(new StoryboardQueueItem(storyboard, element, completedCallback));

            if (this.runningStoryboard == null) {
                StartNextItem();
            }
        }

        private void ItemCompleted(object sender, EventArgs e) {
            var item = (StoryboardQueueItem) sender;

            var callback = item.CompletedCallback;

            if (callback != null) {
                callback();
            }

            if (queue.Any()) {
                StartNextItem();
            } else {
                this.runningStoryboard = null;
            }
        }

        private void StartNextItem() {
            var item = queue.Dequeue();

            this.runningStoryboard = item.Storyboard;

            item.Storyboard.Dispatcher.BeginInvoke(new Action(() => {
                EventHandler completedCallback = null;

                completedCallback = (sender, e) => {
                    item.Storyboard.Completed -= completedCallback;

                    ItemCompleted(item, e);
                };

                item.Storyboard.Completed += completedCallback;

                item.Storyboard.Begin(item.Element);
            }));
        }
    }
}