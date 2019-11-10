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
                mainScollbar.ScrollToRightEnd();
            }
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(this);
            double x = p.X + mainScollbar.HorizontalOffset;
            double y = p.Y;

            if (isBetweenElementsOrAtEnd(x, y))
            {
                _resizing = GetElementAt(x - 15, y);
            }
            else if (GetElementAt(x, y) != null)
            {
                Mouse.OverrideCursor = Cursors.Cross;
                _moving = GetElementAt(x, y);
                _movingOffset = x - GetElementAt(x, y).StartTime;
                _moving.Grabbed = true;
            }
        }

        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(this);
            double x = p.X + mainScollbar.HorizontalOffset;
            double y = p.Y;

            if (!isBetweenElementsOrAtEnd(x, y))
            {
                //Recolor Element
                TimelineElementControl element = GetElementAt(p.X + mainScollbar.HorizontalOffset, p.Y);
                if (element == null)
                {
                    return;
                }
                element.tlElementContent.Background = new SolidColorBrush(Color.FromRgb((byte)_rnd.Next(256), (byte)_rnd.Next(256), (byte)_rnd.Next(256)));

            }
        }

        private double GetLastElementEndtime()
        {
            double currentLongest = -1;
            foreach (TimelineElementControl element in Elements)
            {
                if (element.EndTime > currentLongest)
                    currentLongest = element.EndTime;
            }

            return currentLongest;
        }

        private TimelineElementControl GetElementAt(double x, double y)
        {
            TimelineElementControl fittingElement = null;
            foreach (TimelineElementControl element in Elements)
            {
                if (x >= element.StartTime && x <= element.EndTime
                    && y > 5 && y <= 5 + element.ElementHeight)
                {
                    fittingElement = element;
                    break;
                }
            }

            return fittingElement;
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(this);
            double x = p.X + mainScollbar.HorizontalOffset;
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
                    mainCanvas.Width = _resizing.EndTime + 100;
                }
                _resizing.resizeAndPush(x);
            }

            if (_moving != null)
            {
                Mouse.OverrideCursor = Cursors.Cross;
                _moving.moveAndSwap(x, _movingOffset);
            }
        }

        private bool isBetweenElementsOrAtEnd(double x, double y)
        {
            return isBetweenElements(x, y) || (x >= GetLastElementEndtime() - 4 && x <= GetLastElementEndtime() + 8);
        }

        private bool isBetweenElements(double x, double y)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                TimelineElementControl element = Elements[i];
                if (x >= element.StartTime - 4 && x <= element.StartTime + 10
                    && y > 5 && y <= 5 + element.ElementHeight)
                {
                    return true;
                }
            }
            return false;
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_moving != null)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                _moving.Grabbed = false;
                pack();
            }
            _resizing = null;
            _moving = null;
            mainCanvas.Width = GetLastElementEndtime() + 100;
        }

        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
            _resizing = null;
            _moving = null;
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mainScollbar.ScrollToHorizontalOffset(mainScollbar.HorizontalOffset - e.Delta);
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
    }
}
