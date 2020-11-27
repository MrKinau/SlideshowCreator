using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for TimelinePictureElement
    /// </summary>
    public partial class TimelinePictureElementControl : UserControl
    {

        private TimelineControl timeline;
        private BackgroundWorker loadWorker;
        private BitmapImage _loadingImg;

        public double StartTime;
        public double EndTime;

        public double ElementHeight;
        public double TopSpacing;

        public bool Grabbed = false;
        public string Thumbnail;

        public Transition Transition;

        /// <summary>
        /// Initializing new picture element
        /// </summary>
        /// <param name="timeline">added element to timeline</param>
        /// <param name="startTime">element start time</param>
        /// <param name="endTime">element end time</param>
        /// <param name="thumbnail">element thumbnail path</param>
        public TimelinePictureElementControl(TimelineControl timeline, double startTime, double endTime, string thumbnail)
        {
            InitializeComponent();
            this.timeline = timeline;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Thumbnail = thumbnail;

            timeline.MainCanvas.Children.Add(this);
            UpdateHeight();
            Update();

            loadWorker = new BackgroundWorker();
            loadWorker.DoWork += loadWorker_work;
            loadWorker.RunWorkerCompleted += loadWorker_completed;
            loadWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Positioning the element
        /// </summary>
        public void Update()
        {
            TLElementContent.Width = EndTime - StartTime;
            TLElementContent.Height = ElementHeight;
            if (Grabbed)
                Canvas.SetZIndex(this, timeline.PictureElements.Count);
            else
                Canvas.SetZIndex(this, timeline.PictureElements.IndexOf(this));
            Canvas.SetLeft(this, StartTime);
            Canvas.SetTop(this, TopSpacing);

            TimelineSettingsControl tlControls = ((MainWindow)Application.Current.MainWindow).timelineControls;
            if (tlControls == null || tlControls.DisplayTime.IsEnabled == false || timeline.Marked == null || timeline.Marked != this)
                return;
            int newValue = ((int)Math.Floor(EndTime - StartTime)) * 10;
            if (tlControls.DisplayTime.Value != newValue)
                tlControls.DisplayTime.Value = newValue;
        }

        /// <summary>
        /// Updating element height
        /// </summary>
        public void UpdateHeight()
        {
            this.ElementHeight = 60 > Math.Min(timeline.MainCanvas.ActualHeight / 2.0 , 100) ? 60 : Math.Min(timeline.MainCanvas.ActualHeight / 2.0, 100);
            this.TopSpacing = ((timeline.MainCanvas.ActualHeight / 2.0) - (ElementHeight / 2.0));
        }

        /// <summary>
        /// Move and swap elements
        /// </summary>
        /// <param name="newStartTime">new start position for the element</param>
        /// <param name="movingOffset">offset of the mouse to the element</param>
        public void MoveAndSwap(double newStartTime, double movingOffset)
        {
            double length = EndTime - StartTime;
            StartTime = newStartTime - movingOffset;
            EndTime = StartTime + length;
            Swap();
        }

        /// <summary>
        /// Resize the element and push all following elements further
        /// </summary>
        /// <param name="newEndSize">new resultant size</param>
        public void ResizeAndPush(double newEndSize)
        {
            EndTime = (newEndSize - StartTime) > 20 ? newEndSize : StartTime + 20;
            PackFollowing();
        }

        /// <summary>
        /// Pack following elements to fit without gaps and overlaps
        /// </summary>
        public void PackFollowing()
        {
            Update();

            TimelinePictureElementControl lastElement = this;
            foreach (TimelinePictureElementControl element in timeline.PictureElements)
            {
                if (element.StartTime > StartTime)
                {
                    double alltime = element.EndTime - element.StartTime;
                    element.StartTime = lastElement.EndTime;
                    element.EndTime = element.StartTime + alltime;
                    element.Update();
                    lastElement = element;
                }
            }
        }

        /// <summary>
        /// Swap With element (before or after)
        /// </summary>
        public void Swap()
        {
            Update();
            int myIndex = timeline.PictureElements.IndexOf(this);

            if(timeline.PictureElements.Count > myIndex + 1 && StartTime > timeline.PictureElements[myIndex + 1].StartTime)
            {
                timeline.PictureElements[myIndex] = timeline.PictureElements[myIndex + 1];
                timeline.PictureElements[myIndex + 1] = this;
                timeline.Pack();
            }
            else if (myIndex != 0 && StartTime < timeline.PictureElements[myIndex - 1].StartTime)
            {
                timeline.PictureElements[myIndex] = timeline.PictureElements[myIndex - 1];
                timeline.PictureElements[myIndex - 1] = this;
                timeline.Pack();
            }
        }

        /// <summary>
        /// Mark element as selected and enable TimelineControls
        /// </summary>
        public void Select()
        {
            TimelineSettingsControl tlControls = ((MainWindow) Application.Current.MainWindow).timelineControls;
            if (tlControls == null)
                return;
            tlControls.DisplayTime.IsEnabled = true;
            tlControls.Transition.IsEnabled = true;
            tlControls.TransitionTime.IsEnabled = Transition != null;

            tlControls.DisplayTime.Value = (int)Math.Floor(EndTime - StartTime) * 10;
            tlControls.Transition.SelectedIndex = Transition.getID(Transition);
            tlControls.TransitionTime.Value = Transition == null ? 0 : Transition.ExecutionTime;
        }

        /// <summary>
        /// Loading thumbnail
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void loadWorker_work(object sender, DoWorkEventArgs e)
        {
            if (Thumbnail != null)
                _loadingImg = ImageConverter.ScaleToBitmapImage(new Uri(Thumbnail), (int)(EndTime - StartTime), (int)Math.Floor(ElementHeight));
            else
                _loadingImg = ImageConverter.ToBitmapImage(ImageConverter.CreateBlankImage((int)(EndTime - StartTime) - 50, (int)Math.Floor(ElementHeight)));
        }

        /// <summary>
        /// Completing thumbnail load
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void loadWorker_completed(object sender, AsyncCompletedEventArgs e)
        {
            DisplayedImage.Source = _loadingImg;
            timeline.UpdatePreview();
        }

        /// <summary>
        /// Delete this element
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            _loadingImg = null;
            if (loadWorker != null && loadWorker.IsBusy)
            {
                loadWorker.CancelAsync();
                loadWorker = null;
            }

            timeline.RemovePictureElement(this);
            timeline = null;
            Thumbnail = null;
        }
    }
}
