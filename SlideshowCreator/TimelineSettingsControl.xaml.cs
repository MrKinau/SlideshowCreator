using System.Windows;
using System.Windows.Controls;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for TimelineSettings
    /// </summary>
    public partial class TimelineSettingsControl : UserControl
    {
        public TimelineSettingsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Changes the display time
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void DisplayTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TimelineControl timeline = ((MainWindow)Application.Current.MainWindow).timeline;
            if (timeline == null)
                return;
            timeline.Marked.ResizeAndPush(timeline.Marked.StartTime + ((DisplayTime.Value ?? 0) / 10));
        }

        /// <summary>
        /// Changes the selected transition
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void Transition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TimelineControl timeline = ((MainWindow)Application.Current.MainWindow).timeline;
            if (timeline == null)
                return;
            Transition possibleNewTransition = SlideshowCreator.Transition.getByID(Transition.SelectedIndex);
            int oldTime = timeline.Marked.Transition == null ? -1 : timeline.Marked.Transition.ExecutionTime;
            if (timeline.Marked.Transition == null || possibleNewTransition == null || timeline.Marked.Transition.GetType() != possibleNewTransition.GetType())
            {
                timeline.Marked.Transition = possibleNewTransition;
                if (oldTime > 0 && possibleNewTransition != null)
                    timeline.Marked.Transition.ExecutionTime = oldTime;
            }

            TransitionTime.IsEnabled = timeline.Marked.Transition != null;
        }

        /// <summary>
        /// Transition time changed
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void TransitionTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TimelineControl timeline = ((MainWindow)Application.Current.MainWindow).timeline;
            if (timeline == null)
                return;
            if (timeline.Marked.Transition != null)
                timeline.Marked.Transition.ExecutionTime = TransitionTime.Value ?? 1000;
        }
    }
}
