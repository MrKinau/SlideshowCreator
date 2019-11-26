﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TimelineControl : UserControl
    {

        private TimelinePictureElementControl _resizing = null;
        private TimelinePictureElementControl _marked = null;
        private double _movingOffset = 0;
        private bool is_preview = false;
        private readonly Random _rnd = new Random();
        private bool _mayMove = false;

        public List<TimelinePictureElementControl> PictureElements = new List<TimelinePictureElementControl>();
        public List<TimelineMusicElementControl> MusicElements = new List<TimelineMusicElementControl>();
        public List<MediaPlayer> MusicMediaElements = new List<MediaPlayer>();
        

        public TimelineControl()
        {
            InitializeComponent();
        }

        /**
         *  public methods
         */

        public void AddPictureElement(double startTime, double endTime, string thumbnail)
        {
            TimelinePictureElementControl element = new TimelinePictureElementControl(this, startTime, endTime, thumbnail);
            PictureElements.Add(element);

            if (mainCanvas.ActualWidth < GetLastPictureElementEndtime())
            {
                mainCanvas.Width = GetLastPictureElementEndtime() + 100;
                mainScrollbar.ScrollToRightEnd();
            }
        }

        public void AddPictureElement(string imgPath)
        {
            AddPictureElement(GetLastPictureElementEndtime(), GetLastPictureElementEndtime() + 200, imgPath);
        }

        public void AddMusicElement(string musicPath)
        {
            TimelineMusicElementControl element = new TimelineMusicElementControl(this, GetLastMusicElementEndtime(), GetLastMusicElementEndtime() +200, musicPath);
            MediaPlayer media = new MediaPlayer();
            media.Open(new Uri(musicPath, UriKind.Relative));
            media.Play();
            MusicElements.Add(element);
            MusicMediaElements.Add(media);
            if (mainCanvas.ActualWidth < GetLastPictureElementEndtime())
            {
                mainCanvas.Width = GetLastPictureElementEndtime() + 100;
                mainScrollbar.ScrollToRightEnd();
            }
        }

        public void RemovePictureElement(TimelinePictureElementControl element)
        {
            if (!PictureElements.Contains(element))
                return;
            PictureElements.Remove(element);
            Pack();
            updateDrawings();
        }

        public void Pack()
        {
            if (PictureElements.Count < 1)
                return;

            TimelinePictureElementControl lastElement = null;
            foreach (TimelinePictureElementControl element in PictureElements)
            {

                double alltime = element.EndTime - element.StartTime;
                element.StartTime = lastElement == null ? 0 : lastElement.EndTime;
                element.EndTime = element.StartTime + alltime;
                element.update();
                lastElement = element;
            }
        }

        public void UpdatePreview()
        {
            ///\todo make MVVM
            if (GetPictureElementAtMarker() == null || GetPictureElementAtMarker().Thumbnail == null)
                return;
            string thumbnail = GetPictureElementAtMarker().Thumbnail;
            PreviewControl preview = ((MainWindow)Application.Current.MainWindow).preview;
            preview.UpdateImageAsync(thumbnail);
        }

        /**
         *  private methods
         */

        //TODO: Make more compact
        private string timeToString(int time)
        {
            string timeStr = "";
            int minutes = 0;
            int seconds = 0;

            time /= 100;

            while (time > 60)
            {
                minutes++;
                time -= 60;
            }

            seconds = time;

            if (minutes < 10)
            {
                timeStr += "0";
                if (minutes == 0)
                {
                    timeStr += "0";
                }
                else
                {
                    timeStr += minutes;
                }
            }
            else
                timeStr += minutes.ToString();

            timeStr += ":";

            if (seconds < 10)
            {
                timeStr += "0";
                if (seconds == 0)
                {
                    timeStr += "0";
                }
                else
                {
                    timeStr += seconds;
                }
            }
            else
                timeStr += seconds.ToString();

            return timeStr;
        }

        private double GetLastPictureElementEndtime()
        {
            if (PictureElements.Count > 0)
            {
                TimelinePictureElementControl last = PictureElements[PictureElements.Count - 1];
                return last.EndTime;
            }
            return 0;
        }

        private double GetLastMusicElementEndtime()
        {
            if (MusicElements.Count > 0)
            {
                TimelineMusicElementControl last = MusicElements[MusicElements.Count - 1];
                return last.EndTime;
            }
            return 0;
        }

        private TimelinePictureElementControl GetPictureElementAt(double x, double y)
        {
            TimelinePictureElementControl fittingElement = null;
            foreach (TimelinePictureElementControl element in PictureElements)
            {
                if (x >= element.StartTime && x <= element.EndTime
                    && y > element.TopSpacing && y <= element.TopSpacing + element.ElementHeight)
                {
                    fittingElement = element;
                    break;
                }
            }

            return fittingElement;
        }

        private TimelinePictureElementControl GetPictureElementAtMarker()
        {
            int left = (int)Math.Round(Canvas.GetLeft(tlMarker)) + 5;
            Console.WriteLine("left: " + left + ", elements: " + PictureElements.Count);
            TimelinePictureElementControl fittingElement = null;
            foreach (TimelinePictureElementControl element in PictureElements)
            {
                if (left >= element.StartTime && left <= element.EndTime)
                {
                    fittingElement = element;
                    break;
                }
            }

            return fittingElement;
        }
        private TimelineMusicElementControl isAtMusicElement(double x, double y)
        {
            TimelineMusicElementControl selectelement = null;
            foreach(TimelineMusicElementControl element in MusicElements)
            {
                if (x >= element.StartTime && x <= (element.EndTime - 17)
                    && y > element.TopSpacing && y <= element.TopSpacing + element.ElementHeight)
                    selectelement = element;
                Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxxxx");

            }
            return selectelement;
        }
        private void MoveMarker(int x)
        {
            Canvas.SetLeft(tlMarker, Math.Max(x - 5, -5));
            TimelinePictureElementControl elementAtMarker = GetPictureElementAtMarker();
            if (elementAtMarker == null)
                return;
            UpdatePreview();
        }

        private bool isBetweenPictureElements(double x, double y)
        {
            for (int i = 0; i < PictureElements.Count; i++)
            {
                TimelinePictureElementControl element = PictureElements[i];
                if (x >= element.StartTime - 6 && x <= element.StartTime + 6
                    && y > element.TopSpacing && y <= element.TopSpacing + element.ElementHeight)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isBetweenPictureElementsOrAtEnd(double x, double y)
        {
            bool between = isBetweenPictureElements(x, y);
            bool end = false;
            if (PictureElements.Count > 0)
            {
                TimelinePictureElementControl last = PictureElements[PictureElements.Count - 1];
                end = x >= last.EndTime - 6 && x <= last.EndTime + 6
                    && y > last.TopSpacing && y <= last.TopSpacing + last.ElementHeight;
            }
            return between || end;
        }

        private void cancelAllActions()
        {
            if (_mayMove == true)
            {
                _marked.Grabbed = false;
                Pack();
            }
            _resizing = null;
            _mayMove = false;
            Mouse.OverrideCursor = null;
            mainCanvas.Width = ActualWidth > GetLastPictureElementEndtime() + 100 ? ActualWidth : GetLastPictureElementEndtime() + 100;
            Pack();
            updateDrawings();
        }

        private bool isVisible(TimelinePictureElementControl element)
        {
            if (element.EndTime > mainScrollbar.HorizontalOffset && element.StartTime < mainScrollbar.HorizontalOffset + mainScrollbar.ActualWidth)
            {
                return true;
            }
            return false;

        }

        private bool isVisible(TimelineMusicElementControl element)
        {
            if (element.EndTime > mainScrollbar.HorizontalOffset && element.StartTime < mainScrollbar.HorizontalOffset + mainScrollbar.ActualWidth)
            {
                return true;
            }
            return false;

        }

        private void updateDrawings()
        {
            //Delete old timestamplines & timestamptexts & invisible TimelineElements
            List<UIElement> toRemove = new List<UIElement>();
            foreach (UIElement child in mainCanvas.Children)
            {
                if (child.GetType() == typeof(Line) || child.GetType() == typeof(Label) || child.GetType() == typeof(TimelinePictureElementControl) || child.GetType() == typeof(TimelineMusicElementControl))
                {
                    toRemove.Add(child);
                }
            }

            foreach (UIElement element in toRemove)
            {
                mainCanvas.Children.Remove(element);
            }

            //Draw TimelineElements

            foreach (TimelinePictureElementControl element in PictureElements)
            {
                if (isVisible(element))
                {
                    mainCanvas.Children.Add(element);
                    element.update();
                }
            }

            foreach (TimelineMusicElementControl element in MusicElements)
            {
                if (isVisible(element))
                {
                    mainCanvas.Children.Add(element);
                    element.updateHeight();
                    element.update();
                }
            }

            //Draw every second timestamp text


            for (int i = 0; i < mainCanvas.Width; i += 100)
            {
                if (i < mainScrollbar.HorizontalOffset - 100)
                    continue;
                if (i > mainScrollbar.HorizontalOffset + mainScrollbar.ActualWidth + 100)
                    break;

                Label label = new Label();
                label.Content = timeToString(i);
                label.Padding = new Thickness(0);
                label.Margin = new Thickness(0, -5, 0, 0);
                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                label.Foreground = new SolidColorBrush(Colors.White);

                Size s = label.DesiredSize;

                mainCanvas.Children.Add(label);
                Canvas.SetLeft(label, i - (s.Width / 2.0));
                Canvas.SetTop(label, 0);
            }
            
            

            //Draw 10th of second timestamp line

            for (int i = 0; i < mainCanvas.Width; i += 10)
            {
                if (i < mainScrollbar.HorizontalOffset - 100)
                    continue;
                if (i > mainScrollbar.HorizontalOffset + mainScrollbar.ActualWidth + 100)
                    break;
                Line line = new Line();
                line.Stroke = new SolidColorBrush(Colors.White);
                line.X1 = i;
                line.X2 = i;
                line.Y2 = 20;
                if (i % 100 == 0)
                {
                    line.StrokeThickness = 0.5;
                    line.Y1 = 10;
                }
                else
                {
                    line.StrokeThickness = 0.4;
                    line.Y1 = 13;
                }

                mainCanvas.Children.Add(line);
            }

            Line bottomLine = new Line();
            bottomLine.X1 = mainScrollbar.HorizontalOffset;
            bottomLine.X2 = mainScrollbar.HorizontalOffset + mainScrollbar.ActualWidth;
            bottomLine.Y1 = 20;
            bottomLine.Y2 = 20;
            bottomLine.StrokeThickness = 0.5;
            bottomLine.Stroke = new SolidColorBrush(Colors.White);

            mainCanvas.Children.Add(bottomLine);
        }

        /**
         *  events
         */

        private void MainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            mainCanvas.Width = ActualWidth - 10;
            Canvas.SetZIndex(tlMarker, int.MaxValue);
            Canvas.SetLeft(tlMarker, -5);
            tlMarker.markerLine.Y2 = ActualHeight - 10;
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Point p = Mouse.GetPosition(mainCanvas);
            double x = p.X;
            double y = p.Y;

            if (isBetweenPictureElementsOrAtEnd(x, y))
            {
                _resizing = GetPictureElementAt(x - 15, y);
            }
            else if (GetPictureElementAt(x, y) != null)
            {
                if (_marked != null)
                    _marked.Border.Background = FindResource("SC_BG_COLOR_BRIGHT") as SolidColorBrush;
                _marked = GetPictureElementAt(x, y);
                _marked.Border.Background = FindResource("SC_BG_COLOR_MEDIUM") as SolidColorBrush;
                _mayMove = true;
                _movingOffset = x - GetPictureElementAt(x, y).StartTime;
            }
            else if (isAtMusicElement(x, y) != null)
            {
                TimelineMusicElementControl ElementSelect = isAtMusicElement(x, y);
                ElementSelect.MovingMusicElement((int)Math.Round(x));
            }
            else
            {
                MoveMarker((int)Math.Round(x));
            }
        }

       

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            cancelAllActions();
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(mainCanvas);
            double x = p.X;
            double y = p.Y;

            //change cursor to corresponding: resize, move elemenent
            if (_mayMove == false && (isBetweenPictureElementsOrAtEnd(x, y) || _resizing != null))
            {
                Mouse.OverrideCursor = Cursors.SizeWE;
            } 
            else if (_mayMove == false)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                _resizing = null;
            }

            //resize element
            if (_resizing != null)
            {
                if (_resizing.EndTime == GetLastPictureElementEndtime())
                {
                    mainCanvas.Width = ActualWidth > _resizing.EndTime + 100 ? ActualWidth : _resizing.EndTime + 100;
                }
                _resizing.resizeAndPush(x);
                updateDrawings();
                return;
            }

            //move element
            if (_mayMove == true)
            {
                Mouse.OverrideCursor = ((TextBlock)Resources["CursorGrabbing"]).Cursor;
                _marked.moveAndSwap(x, _movingOffset);
                _marked.Grabbed = true;
                return;
            }

            //move marker
            if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                MoveMarker((int)Math.Round(x));
            }
        }



        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            cancelAllActions();
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mainScrollbar.ScrollToHorizontalOffset(mainScrollbar.HorizontalOffset - e.Delta);
        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateDrawings();
            tlMarker.markerLine.Y2 = ActualHeight - 10;
        }

        private void MainScrollbar_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            updateDrawings();
        }

        public void preview()
        {
            if (!is_preview)
            {
                if (PictureElements.Count == 0) return;
                else
                {
                    bool is_music_on = false;
                    int index = 0;
                    int last = (int)GetLastPictureElementEndtime()-5;
                    int i = 5;
                    while (i < last){
                        Canvas.SetLeft(tlMarker, (double)i);
                        UpdatePreview();
                        i++;
                        
                    }
                    /*hier wird irgendwie abgespiel*/
                    int left = (int)Math.Round(Canvas.GetLeft(tlMarker)) + 5;
                   
                    foreach (TimelineMusicElementControl element in MusicElements)
                    {
                        
                        if(element.StartTime == left)
                        {
                            if (is_music_on) {
                                MusicMediaElements[index].Stop();
                                index = MusicElements.IndexOf(element);
                                MusicMediaElements[index].Play();
                            }
                            else
                            {
                                is_music_on = true;
                                index = MusicElements.IndexOf(element);
                                MusicMediaElements[index].Play();
                            }
                            
                        }
                    }
                }
            }

        }
     
    }
}
