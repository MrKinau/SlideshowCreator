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

        private readonly Random _rnd = new Random();

        public List<TimelineElementControl> Elements = new List<TimelineElementControl>();

        public TimelineControl()
        {
            InitializeComponent();
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TimelineElementControl picture = new TimelineElementControl(this, GetLastElementEndtime(), GetLastElementEndtime() + _rnd.Next(201) + 50);
            Elements.Add(picture);

            //Random color
            picture.tlElementContent.Fill = new SolidColorBrush(Color.FromRgb((byte)_rnd.Next(256), (byte)_rnd.Next(256), (byte)_rnd.Next(256)));

            if (mainCanvas.ActualWidth < GetLastElementEndtime())
            {
                mainCanvas.Width = GetLastElementEndtime();
            }
        }

        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(this);
            TimelineElementControl element = GetElementAt(p.X + mainScollbar.HorizontalOffset, p.Y);
            if (element == null)
            {
                return;
            }
            element.tlElementContent.Fill = new SolidColorBrush(Color.FromRgb((byte)_rnd.Next(256), (byte)_rnd.Next(256), (byte)_rnd.Next(256)));
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
    }
}
