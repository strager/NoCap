using System.Windows;
using System.Windows.Media.Animation;

namespace NoCap.Extensions.Default.Helpers {
    internal class StoryboardQueueItem {
        private readonly Storyboard storyboard;
        private readonly FrameworkElement element;

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

        public StoryboardQueueItem(Storyboard storyboard, FrameworkElement element) {
            this.storyboard = storyboard;
            this.element = element;
        }
    }
}