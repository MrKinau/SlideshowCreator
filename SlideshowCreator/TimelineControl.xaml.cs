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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TimelineControl : UserControl
    {

        private TimelineElementControl _resizing = null;
        private TimelineElementControl _moving = null;
        private double _movingOffset = 0;
        private readonly Random _rnd = new Random();

        public List<TimelineElementControl> Elements = new List<TimelineElementControl>();

        public TimelineControl()
        {
            InitializeComponent();
        }

        /**
         *  public methods
         */

        public void addTestElement()
        {
            //Create Element
            TimelineElementControl picture = new TimelineElementControl(this, GetLastElementEndtime(), GetLastElementEndtime() + _rnd.Next(121) + 80);
            Elements.Add(picture);

            //Random color
            picture.tlElementContent.Background = new SolidColorBrush(Color.FromRgb((byte)_rnd.Next(256), (byte)_rnd.Next(256), (byte)_rnd.Next(256)));

            if (mainCanvas.ActualWidth < GetLastElementEndtime())
            {
                mainCanvas.Width = GetLastElementEndtime() + 100;
                mainScrollbar.ScrollToRightEnd();
            }
        }

        public void pack()
        {
            if (Elements.Count < 1)
                return;

            TimelineElementControl lastElement = null;
            foreach (TimelineElementControl element in Elements)
            {

                double alltime = element.EndTime - element.StartTime;
                element.StartTime = lastElement == null ? 0 : lastElement.EndTime;
                element.EndTime = element.StartTime + alltime;
                element.update();
                lastElement = element;
            }
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

        private double GetLastElementEndtime()
        {
            if (Elements.Count > 0)
            {
                TimelineElementControl last = Elements[Elements.Count - 1];
                return last.EndTime;
            }
            return 0;
        }

        private TimelineElementControl GetElementAt(double x, double y)
        {
            TimelineElementControl fittingElement = null;
            foreach (TimelineElementControl element in Elements)
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

        private bool isBetweenElements(double x, double y)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                TimelineElementControl element = Elements[i];
                if (x >= element.StartTime - 4 && x <= element.StartTime + 10
                    && y > element.TopSpacing && y <= element.TopSpacing + element.ElementHeight)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isBetweenElementsOrAtEnd(double x, double y)
        {
            bool between = isBetweenElements(x, y);
            bool end = false;
            if (Elements.Count > 0)
            {
                TimelineElementControl last = Elements[Elements.Count - 1];
                end = x >= last.EndTime - 4 && x <= last.EndTime + 8
                    && y > last.TopSpacing && y <= last.TopSpacing + last.ElementHeight;
            }
            return between || end;
        }

        private void cancelAllActions()
        {
            if (_moving != null)
            {
                _moving.Grabbed = false;
                pack();
            }
            _resizing = null;
            _moving = null;
            Mouse.OverrideCursor = null;
            pack();
        }

        private bool isVisible(TimelineElementControl element)
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
                if (child.GetType() == typeof(Line) || child.GetType() == typeof(Label) || child.GetType() == typeof(TimelineElementControl))
                {
                    toRemove.Add(child);
                }
            }

            foreach (UIElement element in toRemove)
            {
                mainCanvas.Children.Remove(element);
            }

            //Draw TimelineElements

            foreach (TimelineElementControl element in Elements)
            {
                if (isVisible(element))
                {
                    mainCanvas.Children.Add(element);
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
                line.Stroke = new SolidColorBrush(Colors.Black);
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
            bottomLine.Stroke = new SolidColorBrush(Colors.Black);

            mainCanvas.Children.Add(bottomLine);
        }

        /**
         *  events
         */

        private void MainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            mainCanvas.Width = ActualWidth - 10;
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(this);
            double x = p.X + mainScrollbar.HorizontalOffset;
            double y = p.Y;

            if (isBetweenElementsOrAtEnd(x, y))
            {
                _resizing = GetElementAt(x - 15, y);
            }
            else if (GetElementAt(x, y) != null)
            {
                _moving = GetElementAt(x, y);
                _movingOffset = x - GetElementAt(x, y).StartTime;
                _moving.Grabbed = true;
            }
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            cancelAllActions();
        }

        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(this);
            double x = p.X + mainScrollbar.HorizontalOffset;
            double y = p.Y;

            if (!isBetweenElementsOrAtEnd(x, y))
            {
                //Recolor Element
                TimelineElementControl element = GetElementAt(p.X + mainScrollbar.HorizontalOffset, p.Y);
                if (element == null)
                {
                    return;
                }
                element.tlElementContent.Background = new SolidColorBrush(Color.FromRgb((byte)_rnd.Next(256), (byte)_rnd.Next(256), (byte)_rnd.Next(256)));

            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(this);
            double x = p.X + mainScrollbar.HorizontalOffset;
            double y = p.Y;

            if (isBetweenElementsOrAtEnd(x, y) || _resizing != null)
            {
                Mouse.OverrideCursor = Cursors.SizeWE;
            } 
            else if (_moving == null)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                _resizing = null;
            }

            if (_resizing != null)
            {
                if (_resizing.EndTime == GetLastElementEndtime())
                {
                    mainCanvas.Width = ActualWidth > _resizing.EndTime + 100 ? ActualWidth : _resizing.EndTime + 100;
                }
                _resizing.resizeAndPush(x);
            }

            if (_moving != null)
            {
                Mouse.OverrideCursor = ((TextBlock)Resources["CursorGrabbing"]).Cursor;
                _moving.moveAndSwap(x, _movingOffset);
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
        }

        private void MainScrollbar_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            updateDrawings();
        }
    }
}
