using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace NoCap.Extensions.Default.Helpers {
    internal class StoryboardQueueItem {
        private readonly Storyboard storyboard;
        private readonly FrameworkElement element;
        private readonly Action completedCallback;

        public Storyboard Storyboard {
            get {
                return this.storyboard;
            }
        }

        public FrameworkElement Element {
            get {
                return this.element;
            }
        }

        public Action CompletedCallback {
            get {
                return this.completedCallback;
            }
        }

        public StoryboardQueueItem(Storyboard storyboard, FrameworkElement element, Action completedCallback) {
            this.storyboard = storyboard;
            this.element = element;
            this.completedCallback = completedCallback;
        }
    }
}