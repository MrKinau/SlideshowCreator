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
    /// Interaction logic for TimelineElementControl.xaml
    /// </summary>
    public partial class TimelineElementControl : UserControl
    {

        private TimelineControl timeline;

        public double StartTime;
        public double EndTime;

        public double ElementHeight;
        public double TopSpacing;

        public TimelineElementControl(TimelineControl timeline, double startTime, double endTime)
        {
            InitializeComponent();
            this.timeline = timeline;
            this.StartTime = startTime;
            this.EndTime = endTime;

            timeline.mainCanvas.Children.Add(this);
            updateHeight();
            update();
        }

        public TimelineElementControl()
        {
            InitializeComponent();
        }

        public void update()
        {
            tlElementContent.Width = EndTime - StartTime;
            tlElementContent.Height = ElementHeight;
            Canvas.SetLeft(this, StartTime);
            Canvas.SetTop(this, TopSpacing);
        }

        public void updateHeight()
        {
            this.TopSpacing = timeline.ActualHeight / 10.0;
            this.ElementHeight = timeline.ActualHeight - 27 - 19 - TopSpacing; // -27=Scrollbar, -19=space for music (should be scalable?)
        }

        public void resizeAndPush(double newEndSize)
        {
            EndTime = (newEndSize - StartTime) > 20 ? newEndSize : StartTime + 20;
            update();

            TimelineElementControl lastElement = this;
            foreach (TimelineElementControl element in timeline.Elements)
            {
                if (element.StartTime > StartTime)
                {
                    double alltime = element.EndTime - element.StartTime;
                    element.StartTime = lastElement.EndTime;
                    element.EndTime = element.StartTime + alltime;
                    element.update();
                    lastElement = element;
                }
            }
        }
    }
}
