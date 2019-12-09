using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;

namespace SlideshowCreator
{
    /// <summary>
    /// Logique d'interaction pour Edit_Picture.xaml
    /// </summary>
    public partial class Edit_Picture : Window
    {
        public Edit_Picture()
        {
            InitializeComponent();
        }

        public void set_source(ImageSource source)
        {          
            img.Source = source;
            img.Height = edit_picture_grid.Height;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            img.MaxWidth= edit_picture_grid.ActualWidth - 5 ;
            img.MaxHeight = edit_picture_grid.MaxHeight * 247 / 147.0 - 5;
            Console.WriteLine("New Height " + edit_picture_grid.ActualWidth );
         
        }
    }
}
