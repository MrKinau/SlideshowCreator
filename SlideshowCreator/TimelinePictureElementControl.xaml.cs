using SlideshowCreator.transitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
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

        public TimelinePictureElementControl(TimelineControl timeline, double startTime, double endTime, string thumbnail)
        {
            InitializeComponent();
            this.timeline = timeline;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Thumbnail = thumbnail;

            timeline.mainCanvas.Children.Add(this);
            UpdateHeight();
            Update();

            //Default transition
            Transition = new PushTransition();

            loadWorker = new BackgroundWorker();
            loadWorker.DoWork += loadWorker_work;
            loadWorker.RunWorkerCompleted += loadWorker_completed;
            loadWorker.RunWorkerAsync();
        }

        public void Update()
        {
            tlElementContent.Width = EndTime - StartTime;
            tlElementContent.Height = ElementHeight;
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

        public void UpdateHeight()
        {
            this.ElementHeight = 60 > Math.Min(timeline.mainCanvas.ActualHeight / 2.0 , 100) ? 60 : Math.Min(timeline.mainCanvas.ActualHeight / 2.0, 100);
            this.TopSpacing = ((timeline.mainCanvas.ActualHeight / 2.0) - (ElementHeight / 2.0));
        }

        public void MoveAndSwap(double newStartTime, double movingOffset)
        {
            double length = EndTime - StartTime;
            StartTime = newStartTime - movingOffset;
            EndTime = StartTime + length;
            Swap();
        }

        public void ResizeAndPush(double newEndSize)
        {
            EndTime = (newEndSize - StartTime) > 20 ? newEndSize : StartTime + 20;
            PackFollowing();
        }

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

        private void loadWorker_work(object sender, DoWorkEventArgs e)
        {
            if (Thumbnail != null)
                _loadingImg = ImageConverter.ScaleToBitmapImage(new Uri(Thumbnail), (int)(EndTime - StartTime), (int)Math.Floor(ElementHeight));
            else
                _loadingImg = ImageConverter.ToBitmapImage(ImageConverter.CreateBlankImage((int)(EndTime - StartTime) - 50, (int)Math.Floor(ElementHeight)));
        }

        private void loadWorker_completed(object sender, AsyncCompletedEventArgs e)
        {
            DisplayedImage.Source = _loadingImg;
            timeline.UpdatePreview();
        }

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
