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
using Microsoft.Win32;

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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pictureExplorerPanel.MaxWidth = (e.NewSize.Width - 30) / 1.5;
            pictureExplorerPanel.MinWidth = (e.NewSize.Width - 30) / 4;
            Console.WriteLine("Size changed: " + e.NewSize.Height + "/" + e.NewSize.Width);
        }

        private void Add_Img_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Multiselect = true;
            OpenFile.Title = "Select Picture(s)";
            OpenFile.Filter = "ALL supported Graphics| *.jpeg; *.jpg;*.png;";
            if (OpenFile.ShowDialog() == true)
            {
                foreach(String file in OpenFile.FileNames)
                {
                    Add_Image(file);
                }
            }
        }

        private void Add_Image(string file)
        {
            Image new_img = new Image();
            new_img.Source = new BitmapImage(new Uri(file));
            Thickness img_thickness = new Thickness();
            img_thickness.Bottom = 2;
            img_thickness.Left = 2;
            img_thickness.Right = 2;
            img_thickness.Top = 2;
            new_img.Margin = img_thickness;
            Picture_Holder.Children.Add(new_img);
        }

        private void Timeline_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            { 
                foreach (TimelineElementControl element in timeline.Elements)
                {
                    element.updateHeight();
                    element.update();
                }
            }
            else if (e.WidthChanged)
            {
                if (timeline.mainCanvas.ActualWidth < e.NewSize.Width - 8)
                {
                    timeline.mainCanvas.Width = e.NewSize.Width;
                }
            }
            Console.WriteLine("Size changed: " + e.NewSize.Height + "/" + e.NewSize.Width);
        }

        private void AddTestElement_Click(object sender, RoutedEventArgs e)
        {
            timeline.addTestElement();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PackButton_Click(object sender, RoutedEventArgs e)
        {
            timeline.pack();
        }

        private void Add_Music_Click(object sender, RoutedEventArgs e)
        {

        }

        /* private void Add_Music_Click(object sender, RoutedEventArgs e)
         {
             OpenFileDialog OpenFile = new OpenFileDialog();
             OpenFile.Multiselect = true;
             OpenFile.Title = "Select Picture(s)";
             OpenFile.Filter = "Audio Format| *.mp3; *.wav;*.wma;";
             if (OpenFile.ShowDialog() == true)
             {
                 foreach (String file in OpenFile.FileNames)
                 {
                     Add_Music(file);
                 }
             }
         }*/
        /* private void Add_Music(string file)
         {
             MediaPlayer player = new MediaPlayer();
             player.Open(new Uri(file));
             Music_Holder.Children.Add(player);
         }*/

        private void LsdButton_Click(object sender, RoutedEventArgs e)
        {
            Random _rnd = new Random();
            foreach (TimelineElementControl element in timeline.Elements)
            {
                element.tlElementContent.Background = new SolidColorBrush(Color.FromRgb((byte)_rnd.Next(256), (byte)_rnd.Next(256), (byte)_rnd.Next(256)));
            }
        }
    }
}
