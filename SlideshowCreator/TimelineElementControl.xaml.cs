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

        public Boolean Grabbed = false;

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
            if (Grabbed)
                Canvas.SetZIndex(this, timeline.Elements.Count);
            else
                Canvas.SetZIndex(this, timeline.Elements.IndexOf(this));
            Canvas.SetLeft(this, StartTime);
            Canvas.SetTop(this, TopSpacing);
        }

        public void updateHeight()
        {
            this.ElementHeight = 60 > Math.Min(timeline.mainCanvas.ActualHeight / 2.0 , 100) ? 60 : Math.Min(timeline.mainCanvas.ActualHeight / 2.0, 100);
            this.TopSpacing = ((timeline.mainCanvas.ActualHeight / 2.0) - (ElementHeight / 2.0)) + 10;
        }

        public void moveAndSwap(double newStartTime, double movingOffset)
        {
            double length = EndTime - StartTime;
            StartTime = newStartTime - movingOffset;
            EndTime = StartTime + length;
            swap();
        }

        public void resizeAndPush(double newEndSize)
        {
            EndTime = (newEndSize - StartTime) > 20 ? newEndSize : StartTime + 20;
            packFollowing();
        }

        public void packFollowing()
        {
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


        public void swap()
        {
            update();
            int myIndex = timeline.Elements.IndexOf(this);

            if(timeline.Elements.Count > myIndex + 1 && StartTime > timeline.Elements[myIndex + 1].StartTime)
            {
                timeline.Elements[myIndex] = timeline.Elements[myIndex + 1];
                timeline.Elements[myIndex + 1] = this;
                timeline.pack();
            }
            else if (myIndex != 0 && StartTime < timeline.Elements[myIndex - 1].StartTime)
            {
                timeline.Elements[myIndex] = timeline.Elements[myIndex - 1];
                timeline.Elements[myIndex - 1] = this;
                timeline.pack();
            }
        }
    }
}
