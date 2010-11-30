using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoCap.Extensions.Default.Plugins {
    /// <summary>
    /// Interaction logic for AboutEditor.xaml
    /// </summary>
    public partial class AboutEditor {
        private readonly Feedback feedback = new Feedback();
        private readonly Storyboard feedbackSubmittedStoryboard;

        public Feedback Feedback {
            get {
                return this.feedback;
            }
        }

        internal AboutEditor() {
            InitializeComponent();
            
            // QUICK HACK
            this.feedbackMessage.Opacity = 0;

            this.feedbackSubmittedStoryboard = (Storyboard) Resources["FeedbackSubmittedStoryboard"];
        }

        private void SendFeedback(object sender, RoutedEventArgs e) {
            Feedback.Submit();

            this.feedbackSubmittedStoryboard.Begin(this);
        }
    }
}
