using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace SlideshowCreator
{
    /// <summary>
    /// This control is the bottom part of the application and defines the timeline with all its picture and music components
    /// </summary>
    public partial class TimelineControl : UserControl
    {

        private TimelinePictureElementControl _resizing = null;
        private double _movingOffset = 0;
        private bool _isPreview = false;
        private readonly Random _rnd = new Random();
        private bool _mayMove = false;

        public TimelinePictureElementControl Marked = null;
        public List<TimelinePictureElementControl> PictureElements = new List<TimelinePictureElementControl>();
        public List<TimelineMusicElementControl> MusicElements = new List<TimelineMusicElementControl>();

        public List<TlMusikElementEnde> EndElements = new List<TlMusikElementEnde>();

        
        /// <summary>
        /// Constructor for first init
        /// </summary>
        public TimelineControl()
        {
            InitializeComponent();
        }


        /**
         *  PUBLIC METHODS
         **/

        /// <summary>
        /// Adds a new picture element to the timeline (before the given element)
        /// </summary>
        /// <param name="startTime">the start time for the new element</param>
        /// <param name="endTime">the end time for the new element</param>
        /// <param name="thumbnail">The path to the picture (should be absolute)</param>
        /// <param name="transitionId">The ID of the transition which should be shown before the slide (-1 for no transition)</param>
        /// <param name="transitionExecutionTime">The time the transition should last (in ms). Parameter is ignored if the transition id is invalid</param>
        /// <param name="addBefore">The element id of the element which should be after the added element</param>        
        public void AddPictureElement(double startTime, double endTime, string thumbnail, int transitionId, int transitionExecutionTime, int addBefore)
        {
            TimelinePictureElementControl element = new TimelinePictureElementControl(this, startTime, endTime, thumbnail);
            element.Transition = Transition.getByID(transitionId);
            if (element.Transition != null)
                element.Transition.ExecutionTime = transitionExecutionTime;

            if (addBefore > -1)
            {
                //Push All elements and add new element
                double pushAllTime = endTime - startTime;
                for(int i = addBefore; i < PictureElements.Count; i++)
                {
                    PictureElements[i].StartTime += pushAllTime;
                    PictureElements[i].EndTime += pushAllTime;
                }
                PictureElements.Insert(addBefore, element);
                Pack();
            }
            else
                PictureElements.Add(element);

            if (MainCanvas.ActualWidth < GetLastPictureElementEndtime())
            {
                MainCanvas.Width = GetLastPictureElementEndtime() + 100;
                MainScrollbar.ScrollToRightEnd();
            }

            UpdateExportButton();
        }

        /// <summary>
        /// Adds a new picture element to the timeline
        /// </summary>
        /// <param name="startTime">the start time for the new element</param>
        /// <param name="endTime">the end time for the new element</param>
        /// <param name="thumbnail">The path to the picture (should be absolute)</param>
        public void AddPictureElement(double startTime, double endTime, string thumbnail)
        {
            AddPictureElement(startTime, endTime, thumbnail, -1, 0, -1);
        }

        /// <summary>
        /// Adds a new picture element to the end of the timeline
        /// </summary>
        /// <param name="imgPath">The path to the picture (should be absolute)</param>
        public void AddPictureElement(string imgPath)
        {
            AddPictureElement(GetLastPictureElementEndtime(), GetLastPictureElementEndtime() + 200, imgPath);
        }

        /// <summary>
        /// Adds a new picture element to the timeline (before the given element)
        /// </summary>
        /// <param name="insertImgPath">The path to the picture (should be absolute)</param>
        /// <param name="successor">THe element of the successor picture</param>
        public void AddPictureElement(string insertImgPath, TimelinePictureElementControl successor)
        {
            double newEndTime = successor.StartTime;
            double newStartTime = newEndTime - 200;
            AddPictureElement(newStartTime, newEndTime, insertImgPath, -1, 0, PictureElements.IndexOf(successor));
        }

        /// <summary>
        /// Adds an empty slide (blank, black) at the end
        /// </summary>
        public void AddEmptySlide()
        {
            TimelinePictureElementControl element = new TimelinePictureElementControl(this, GetLastPictureElementEndtime(), GetLastPictureElementEndtime() + 200, null);
            PictureElements.Add(element);

            if (MainCanvas.ActualWidth < GetLastPictureElementEndtime())
            {
                MainCanvas.Width = GetLastPictureElementEndtime() + 100;
                MainScrollbar.ScrollToRightEnd();
            }
        }

        /// <summary>
        /// Adds an music element to the next possible part of the slideshow (concating all audios together)
        /// </summary>
        /// <param name="musicPath">The path to the music track</param>
        public void AddMusicElement(string musicPath)
        {
            TagLib.File file = TagLib.File.Create(musicPath);

            TimelineMusicElementControl element = new TimelineMusicElementControl(this, GetLastMusicElementEndtime(), GetLastMusicElementEndtime() + Convert.ToDouble(file.Properties.Duration.TotalMilliseconds / 10), musicPath);
            TlMusikElementEnde Ende = new TlMusikElementEnde(this, element);
            MusicElements.Add(element);
            EndElements.Add(Ende);
        }

        /// <summary>
        /// Adds an music element to the specified location (only used to restore old music data)
        /// </summary>
        /// <param name="musicPath">The path to the music track</param>
        public void AddMusicElement(string musicPath, double startTime, double endTime)
        {
            TimelineMusicElementControl element = new TimelineMusicElementControl(this, startTime, endTime, musicPath);
            TlMusikElementEnde endElement = new TlMusikElementEnde(this, element);
            MusicElements.Add(element);
            EndElements.Add(endElement);
        }

        /// <summary>
        /// Removes a picture element and re-pack all following elements
        /// </summary>
        /// <param name="element">The element which should be removed</param>
        public void RemovePictureElement(TimelinePictureElementControl element)
        {
            if (!PictureElements.Contains(element))
                return;
            PictureElements.Remove(element);
            Pack();
            UpdateDrawings();
            UpdateExportButton();
        }

        /// <summary>
        /// Removes a music element
        /// </summary>
        /// <param name="element">The element which should be removed</param>
        public void RemoveMusicElement(TimelineMusicElementControl element)
        {
            int index = MusicElements.IndexOf(element);
            MusicElements.Remove(element);
            EndElements.Remove(EndElements[index]);
        }


        /// <summary>
        /// Prevent overlapping picture elements and gaps between picture elements
        /// </summary>
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
                element.Update();
                lastElement = element;
            }
        }

        /// <summary>
        /// Triggers a change of the current preview image to that at the marker
        /// </summary>
        public void UpdatePreview()
        {
            if (GetPictureElementAtMarker() == null)
                return;
            string thumbnail = GetPictureElementAtMarker().Thumbnail;
            PreviewControl preview = ((MainWindow)Application.Current.MainWindow).preview;
            preview.UpdateImageAsync(thumbnail);
        }

        /// <summary>
        /// If an image is added or removed checks if the "Create Slideshow" Button is enabled
        /// </summary>
        public void UpdateExportButton()
        {
            if (PictureElements.Count > 0)
            {
                ((MainWindow)Application.Current.MainWindow).FinishButton.IsEnabled = true;
                ((MainWindow)Application.Current.MainWindow).FinishMenuItem.IsEnabled = true;
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).FinishButton.IsEnabled = false;
                ((MainWindow)Application.Current.MainWindow).FinishMenuItem.IsEnabled = false;
            }
        }

        /// <summary>
        /// Music preview
        /// </summary>
        public void preview()
        {
            if (!_isPreview)
            {
                if (PictureElements.Count == 0) return;
                else
                {
                    int last = (int)GetLastPictureElementEndtime() - 5;
                    int i = 5;
                    while (i < last)
                    {
                        Canvas.SetLeft(tlMarker, (double)i);
                        UpdatePreview();
                        i++;

                    }
                }
            }

        }

        /// <summary>
        /// Updates all drawings:
        /// 1. Removing all (RAM-intensive) components
        /// 2. Add all components which are visible
        /// 3. Draw the timestamp texts and lines
        /// </summary>
        public void UpdateDrawings()
        {
            //Delete old timestamplines & timestamptexts & invisible TimelineElements
            List<UIElement> toRemove = new List<UIElement>();
            foreach (UIElement child in MainCanvas.Children)
            {
                if (child.GetType() == typeof(Line) || child.GetType() == typeof(Label) || child.GetType() == typeof(TimelinePictureElementControl) || child.GetType() == typeof(TimelineMusicElementControl))
                {
                    toRemove.Add(child);
                }
            }

            foreach (UIElement element in toRemove)
            {
                MainCanvas.Children.Remove(element);
            }

            //Draw TimelineElements

            foreach (TimelinePictureElementControl element in PictureElements)
            {
                if (isVisible(element))
                {
                    MainCanvas.Children.Add(element);
                    element.Update();
                }
            }

            foreach (TimelineMusicElementControl element in MusicElements)
            {
                if (isVisible(element))
                {
                    MainCanvas.Children.Add(element);
                    element.updateHeight();
                    element.update();
                }
            }

            //Draw every second timestamp text

            for (int i = 0; i < MainCanvas.Width; i += 100)
            {
                if (i < MainScrollbar.HorizontalOffset - 100)
                    continue;
                if (i > MainScrollbar.HorizontalOffset + MainScrollbar.ActualWidth + 100)
                    break;

                Label label = new Label();
                label.Content = timeToString(i);
                label.Padding = new Thickness(0);
                label.Margin = new Thickness(0, -5, 0, 0);
                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                label.Foreground = new SolidColorBrush(Colors.White);

                Size s = label.DesiredSize;

                MainCanvas.Children.Add(label);
                Canvas.SetLeft(label, i - (s.Width / 2.0));
                Canvas.SetTop(label, 0);
            }



            //Draw 10th of second timestamp line

            for (int i = 0; i < MainCanvas.Width; i += 10)
            {
                if (i < MainScrollbar.HorizontalOffset - 100)
                    continue;
                if (i > MainScrollbar.HorizontalOffset + MainScrollbar.ActualWidth + 100)
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

                MainCanvas.Children.Add(line);
            }

            //Adding horizontal bottom line for timestamps

            Line bottomLine = new Line();
            bottomLine.X1 = MainScrollbar.HorizontalOffset;
            bottomLine.X2 = MainScrollbar.HorizontalOffset + MainScrollbar.ActualWidth;
            bottomLine.Y1 = 20;
            bottomLine.Y2 = 20;
            bottomLine.StrokeThickness = 0.5;
            bottomLine.Stroke = new SolidColorBrush(Colors.White);

            MainCanvas.Children.Add(bottomLine);
        }

        /**
         *  PRIVATE METHODS
         */

        /// <summary>
        /// Converts a given time into (mm:ss)
        /// </summary>
        /// <param name="time">given time in format: 100x = 1s</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the latest picture element endtime
        /// </summary>
        /// <returns>latest picture element endtime</returns>
        private double GetLastPictureElementEndtime()
        {
            if (PictureElements.Count > 0)
            {
                TimelinePictureElementControl last = PictureElements[PictureElements.Count - 1];
                return last.EndTime;
            }
            return 0;
        }

        /// <summary>
        /// Returns the latest music element endtime
        /// </summary>
        /// <returns>latest music element endtime</returns>
        private double GetLastMusicElementEndtime()
        {
            if (MusicElements.Count > 0)
            {
                TimelineMusicElementControl last = MusicElements[MusicElements.Count - 1];
                return last.EndTime;
            }
            return 0;
        }

        /// <summary>
        /// Returns the TimelinePictureElement at given coordinates.
        /// Returns null, if no element is present at the given coordinates
        /// </summary>
        /// <param name="x">The given x-Coordinate</param>
        /// <param name="y">The given y-Coordinate (-1 to disable y check)</param>
        /// <returns></returns>
        private TimelinePictureElementControl GetPictureElementAt(double x, double y)
        {
            TimelinePictureElementControl fittingElement = null;
            foreach (TimelinePictureElementControl element in PictureElements)
            {
                if (x >= element.StartTime && x <= element.EndTime
                    && (y == -1 || (y > element.TopSpacing && y <= element.TopSpacing + element.ElementHeight)))
                {
                    fittingElement = element;
                    break;
                }
            }

            return fittingElement;
        }

        /// <summary>
        /// Returns the picture element at the marker
        /// </summary>
        /// <returns>the picture element at the marker</returns>
        private TimelinePictureElementControl GetPictureElementAtMarker()
        {
            int left = (int)Math.Round(Canvas.GetLeft(tlMarker)) + 5;
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

        /// <summary>
        /// Returns rather the given coordinates are at an music element or not
        /// </summary>
        /// <param name="x">The given x-Coordinate</param>
        /// <param name="y">The given y-Coordinate</param>
        /// <returns></returns>
        private TimelineMusicElementControl isAtMusicElement(double x, double y)
        {
            TimelineMusicElementControl selectelement = null;
            foreach(TimelineMusicElementControl element in MusicElements)
            {
                if (x >= element.StartTime-10 && x <= (element.StartTime + 10)
                    && y > element.TopSpacing && y <= element.TopSpacing + element.ElementHeight)
                    selectelement = element;
            }
            return selectelement;
        }

        /// <summary>
        /// Moved the marker to the given x-Coordinate
        /// </summary>
        /// <param name="x">The coordinate the marker should move to</param>
        private void MoveMarker(int x)
        {
            Canvas.SetLeft(tlMarker, Math.Max(x - 5, -5));
            TimelinePictureElementControl elementAtMarker = GetPictureElementAtMarker();
            if (elementAtMarker == null)
                return;
            UpdatePreview();
        }

        /// <summary>
        /// Returns true if the given coordinates are currently between two picture elements (used for resizing)
        /// </summary>
        /// <param name="x">The given x-Coordinate (typically the mouse cursor)</param>
        /// <param name="y">The given y-Coordinate (typically the mouse cursor)</param>
        /// <returns>the visibility state</returns>
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

        /// <summary>
        /// Returns true if the given coordinates are currently between two picture elements or at the ending element (used for resizing)
        /// </summary>
        /// <param name="x">The given x-Coordinate (typically the mouse cursor)</param>
        /// <param name="y">The given y-Coordinate (typically the mouse cursor)</param>
        /// <returns>the visibility state</returns>
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

        /// <summary>
        /// Cancel all actions: Moving, Resizing. And re-pack all picture elements
        /// </summary>
        private void cancelAllActions()
        {
            if (_mayMove == true)
            {
                Marked.Grabbed = false;
                Pack();
            }
            _resizing = null;
            _mayMove = false;
            Mouse.OverrideCursor = null;
            MainCanvas.Width = ActualWidth > GetLastPictureElementEndtime() + 100 ? ActualWidth : GetLastPictureElementEndtime() + 100;
            Pack();
            UpdateDrawings();
        }

        /// <summary>
        /// Returns if the given picture element is currently in the visible rendered viewport (used for RAM-savings)
        /// </summary>
        /// <param name="element">The picture element to check if it is currently visible</param>
        /// <returns>the visibility state</returns>
        private bool isVisible(TimelinePictureElementControl element)
        {
            if (element.EndTime > MainScrollbar.HorizontalOffset && element.StartTime < MainScrollbar.HorizontalOffset + MainScrollbar.ActualWidth)
            {
                return true;
            }
            return false;

        }

        /// <summary>
        /// Returns if the given music element is currently in the visible rendered viewport (used for RAM-savings)
        /// </summary>
        /// <param name="element">The music element to check if it is currently visible</param>
        /// <returns>the visibility state</returns>
        private bool isVisible(TimelineMusicElementControl element)
        {
            if (element.EndTime > MainScrollbar.HorizontalOffset && element.StartTime < MainScrollbar.HorizontalOffset + MainScrollbar.ActualWidth)
            {
                return true;
            }
            return false;

        }

        /**
         *  EVENTS
         */

        /// <summary>
        /// Sets some values after loaded
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void MainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            MainCanvas.Width = ActualWidth - 10;
            Canvas.SetZIndex(tlMarker, int.MaxValue);
            Canvas.SetLeft(tlMarker, -5);
            tlMarker.MarkerLine.Y2 = ActualHeight - 10;
        }

        /// <summary>
        /// Event for start of resizing, moving and markingof elements
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Point p = Mouse.GetPosition(MainCanvas);
            double x = p.X;
            double y = p.Y;

            if (isBetweenPictureElementsOrAtEnd(x, y))
            {
                _resizing = GetPictureElementAt(x - 15, y);
            }
            else if (GetPictureElementAt(x, y) != null)
            {
                if (Marked != null)
                    Marked.PictureBorder.Background = FindResource("SC_BG_COLOR_BRIGHT") as SolidColorBrush;
                Marked = GetPictureElementAt(x, y);
                Marked.PictureBorder.Background = FindResource("SC_BG_COLOR_MEDIUM") as SolidColorBrush;
                Marked.Select();
                _mayMove = true;
                _movingOffset = x - GetPictureElementAt(x, y).StartTime;
            }
            //else if (isAtMusicElement(x, y) != null)
            //{
            //    TimelineMusicElementControl ElementSelect = isAtMusicElement(x, y);
            //    ElementSelect.MovingMusicElement((int)Math.Round(x));
            //}
            else
            {
                MoveMarker((int)Math.Round(x));
            }
        }

        /// <summary>
        /// Cancel allactions if mouse button is up again
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            cancelAllActions();
        }

        /// <summary>
        /// Moving, resizing elements and moving the cursor event
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(MainCanvas);
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
                    MainCanvas.Width = ActualWidth > _resizing.EndTime + 100 ? ActualWidth : _resizing.EndTime + 100;
                }
                _resizing.ResizeAndPush(x);
                UpdateDrawings();
                return;
            }

            //move element
            if (_mayMove == true)
            {
                Mouse.OverrideCursor = ((TextBlock)Resources["CursorGrabbing"]).Cursor;
                Marked.MoveAndSwap(x, _movingOffset);
                Marked.Grabbed = true;
                return;
            }

            //move marker
            if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                MoveMarker((int)Math.Round(x));
            }
        }

        /// <summary>
        /// If the mouse leaves the canvas, all actions should be cancelled
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            cancelAllActions();
        }

        /// <summary>
        /// Scroll with the mouse scroller the timeline
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainScrollbar.ScrollToHorizontalOffset(MainScrollbar.HorizontalOffset - e.Delta);
        }

        /// <summary>
        /// Render everything correct if the width or height is changed
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDrawings();
            tlMarker.MarkerLine.Y2 = ActualHeight - 10;
        }

        /// <summary>
        /// Render everything correct if the content in the scrollpane is moved
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void MainScrollbar_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateDrawings();
        }

        /// <summary>
        /// The drag and drop implementation for the elements
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            string imgPath = e.Data.GetData(typeof(string)) as string;

            Point p = e.GetPosition(MainCanvas);
            double x = p.X;

            if (imgPath == null || !File.Exists(imgPath))
                return;

            TimelinePictureElementControl targetElement = GetPictureElementAt(x, -1);

            if (targetElement != null)
            {
                AddPictureElement(imgPath, targetElement);
            }
            else
            {
                AddPictureElement(imgPath);
            }
        }
    }
}
