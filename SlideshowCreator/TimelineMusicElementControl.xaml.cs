using System;
using System.Collections.Generic;
using System.IO;
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

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for TimelineMusicElementControl.xaml
    /// </summary>
    public partial class TimelineMusicElementControl : UserControl
    {

        private TimelineControl timeline;

        public double StartTime;
        public double EndTime;

        public double ElementHeight;
        public double TopSpacing;

        public bool Grabbed = false;
        public string MusicPath;

        public TimelineMusicElementControl(TimelineControl timeline, double startTime, double endTime, string musicPath)
        {
            InitializeComponent();
            DataContext = this;
            this.timeline = timeline;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.MusicPath = musicPath;

            timeline.mainCanvas.Children.Add(this);
            updateHeight();
            update();
            TextBlock.ToolTip = Path.GetFileName(musicPath);
        }

        public void update()
        {
            tlElementContent.Width = EndTime - StartTime;
            tlElementContent.Height = ElementHeight;
            if (Grabbed)
                Canvas.SetZIndex(this, timeline.MusicElements.Count);
            else
                Canvas.SetZIndex(this, timeline.MusicElements.IndexOf(this));
            Canvas.SetLeft(this, StartTime);
            Canvas.SetTop(this, TopSpacing);
            Console.WriteLine(Musikline.ActualWidth);
        }

        public void updateHeight()
        {
            this.ElementHeight = 100;
            this.TopSpacing = ((timeline.mainCanvas.ActualHeight / 2.0) - (ElementHeight / 2.0)) + (60 > Math.Min(timeline.mainCanvas.ActualHeight / 2.0, 100) ? 60 : Math.Min(timeline.mainCanvas.ActualHeight / 2.0, 100)) - 20;
        }


        public void MovingMusicElement(int x)
        {
            StartTime = x;
            EndTime = x + 200;
            update();
        }
    }
}
