using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for TimelineSettingsControl.xaml
    /// </summary>
    public partial class TimelineSettingsControl : UserControl
    {
        public TimelineSettingsControl()
        {
            InitializeComponent();
        }

        private void DisplayTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TimelineControl timeline = ((MainWindow)Application.Current.MainWindow).timeline;
            if (timeline == null)
                return;
            timeline.Marked.ResizeAndPush(timeline.Marked.StartTime + ((DisplayTime.Value ?? 0) / 10));
        }

        private void Transition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TimelineControl timeline = ((MainWindow)Application.Current.MainWindow).timeline;
            if (timeline == null)
                return;
            Console.WriteLine("CHANGE");
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
