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
using System.Windows.Shapes;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for ExportVideoProgressWindow.xaml
    /// </summary>
    public partial class ExportVideoProgressWindow : Window
    {

        private VideoCreator _videoCreator;

        public ExportVideoProgressWindow(VideoCreator videoCreator)
        {
            this._videoCreator = videoCreator;
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _videoCreator.stopWorker();
        }
    }
}
