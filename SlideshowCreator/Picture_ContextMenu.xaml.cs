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
    /// Logique d'interaction pour Picture_ContextMenu.xaml
    /// </summary>
    public partial class Picture_ContextMenu : UserControl
    {
        
        public PictureExplorerControl Img;
        

        public Picture_ContextMenu(PictureExplorerControl img, String source)
        {
            DataContext = this;
            InitializeComponent();
            Img = img;
            Add_Image(source);           
            img.Picture_Holder.Children.Add(this);
        }

        private void Add_Image(string file)
        {             
            added_Img.Source = new BitmapImage(new Uri(file, UriKind.Absolute));
            added_Img.Margin = new Thickness(2);
            added_Img.MaxWidth = added_Img.MaxHeight = 100;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Edit_Picture Edit = new Edit_Picture();
            Edit.set_source(added_Img.Source);
            Edit.Show();
        }
    }
}
