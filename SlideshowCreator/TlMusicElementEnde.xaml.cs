using System;
using System.IO;
using System.Windows.Controls;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for TlMusikElementEnde.xaml
    /// </summary>
    public partial class TlMusikElementEnde : UserControl
    {
        private TimelineControl timeline;

        public double StartTime;
        public double EndTime;

        public double ElementHeight;
        public double TopSpacing;

        public string MusicPath;

        public TlMusikElementEnde(TimelineControl timeline, TimelineMusicElementControl start)
        {
            InitializeComponent();
            DataContext = this;
            this.timeline = timeline;
            StartTime = start.EndTime ;
            EndTime = start.EndTime + 20;
            MusicPath = start.MusicPath;
            timeline.MainCanvas.Children.Add(this);
            Updateheight();
            update();
            Text.ToolTip = Path.GetFileName(start.MusicPath);
        }

        /// <summary>
        /// Updates the height
        /// </summary>
        private void Updateheight()
        {
            this.ElementHeight = 100;
            this.TopSpacing = ((timeline.MainCanvas.ActualHeight / 2.0) - (ElementHeight / 2.0)) + (60 > Math.Min(timeline.MainCanvas.ActualHeight / 2.0, 100) ? 60 : Math.Min(timeline.MainCanvas.ActualHeight / 2.0, 100)) - 20;

        }

        /// <summary>
        /// Updates the element
        /// </summary>
        private void update()
        {
            TLElementContentEnd.Width = EndTime - StartTime;
            TLElementContentEnd.Height = ElementHeight;
            Canvas.SetLeft(this, StartTime);
            Canvas.SetTop(this, TopSpacing);
        }

        /// <summary>
        /// Moves the end line
        /// </summary>
        /// <param name="x">x-Axis coordinate</param>
        public void MovingEndLine(int x)
        {
            StartTime = x;
            EndTime = x + 200;
            update();
        }
    }
}
