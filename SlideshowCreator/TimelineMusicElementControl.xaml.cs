using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for the TimelineMusicElement.
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

        /// <summary>
        /// New object of the music element
        /// </summary>
        /// <param name="timeline">timeline the new element to be added</param>
        /// <param name="startTime">start time of the music element</param>
        /// <param name="endTime">end time of the music element</param>
        /// <param name="musicPath">path of the music element</param>
        public TimelineMusicElementControl(TimelineControl timeline, double startTime, double endTime, string musicPath)
        {
            InitializeComponent();
            DataContext = this;
            this.timeline = timeline;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.MusicPath = musicPath;

            timeline.MainCanvas.Children.Add(this);
            updateHeight();
            update();
            Text.ToolTip = Path.GetFileName(musicPath);
        }

        /// <summary>
        /// Positioning the music element correctly
        /// </summary>
        public void update()
        {
            TLElementContent.Width = EndTime - StartTime;
            TLElementContent.Height = ElementHeight;
            Canvas.SetLeft(this, StartTime);
            Canvas.SetTop(this, TopSpacing);
        }

        /// <summary>
        /// Setting the correct height for the element
        /// </summary>
        public void updateHeight()
        {
            this.ElementHeight = 200;
            this.TopSpacing = ((timeline.MainCanvas.ActualHeight / 2.0) - (ElementHeight / 2.0)) + (60 > Math.Min(timeline.MainCanvas.ActualHeight / 2.0, 100) ? 60 : Math.Min(timeline.MainCanvas.ActualHeight / 2.0, 100)) - 20;
        }

        public void MovingMusicElement(int x)
        {
            StartTime = x;
            EndTime = x + 200;
            update();
        }

        /// <summary>
        /// Deletes a music element and end element
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">args</param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {            
            foreach(TlMusikElementEnde endElement in timeline.EndElements)
            {
                if(endElement.StartTime == EndTime)
                {
                    timeline.MainCanvas.Children.Remove(endElement);
                    break;
                }
            }

            timeline.RemoveMusicElement(this);


            timeline.UpdateDrawings();
        }
    }
}
