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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TabControl pictureExplorer = (TabControl)FindName("pictureExplorerPanel");
            pictureExplorer.Width = (e.NewSize.Width - 30) / 2;
            Console.WriteLine("Size changed: " + e.NewSize.Height + "/" + e.NewSize.Width);
        }

        private void Timeline_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TimelineControl timeline = (TimelineControl)FindName("timeline");
            if (e.HeightChanged)
            {
                foreach (TimelineElementControl element in timeline.Elements)
                {
                    element.updateHeight();
                    element.update();
                }
            }
            Console.WriteLine("Size changed: " + e.NewSize.Height + "/" + e.NewSize.Width);
        }
    }
}
