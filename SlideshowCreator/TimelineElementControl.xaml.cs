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

        public TimelineElementControl(TimelineControl timeline, double startTime, double endTime)
        {
            InitializeComponent();
            this.timeline = timeline;
            this.StartTime = startTime;
            this.EndTime = endTime;

            updateHeight();
            update();
            timeline.mainCanvas.Children.Add(this);
            Canvas.SetLeft(this, startTime);
        }

        public TimelineElementControl()
        {
            InitializeComponent();
        }

        public void update()
        {
            tlElementContent.Width = EndTime - StartTime;
            tlElementContent.Height = ElementHeight;
        }

        public void updateHeight()
        {
            this.ElementHeight = timeline.ActualHeight - 27 - 19; // -27=Scrollbar, -19=space for music (should be scalable?)
        }
    }
}
