using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for PreviewControl.xaml
    /// </summary>
    public partial class PreviewControl : UserControl
    {

        private BitmapImage _bitmap;
        private string _imgSrc;

        public PreviewControl()
        {
            InitializeComponent();
        }

        public void UpdateImageAsync(string newImg)
        {
            UpdateImageAsync(newImg, false);
        }

        public void UpdateImageAsync(string newImg, bool force)
        {
            if (!force && newImg != null && newImg.Equals(_imgSrc))
                return;

            _imgSrc = newImg;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += worker_work;
            worker.RunWorkerCompleted += worker_completed;
            worker.RunWorkerAsync();
        }

        private void worker_work(object sender, DoWorkEventArgs e)
        {
            if (ActualWidth > 0 && ActualHeight > 0)
            {
                if (_imgSrc != null)
                    _bitmap = ImageConverter.ScaleToBitmapImage(new Uri(_imgSrc), (int)Math.Floor(ActualWidth), (int)Math.Floor(ActualHeight));
                else
                    _bitmap = ImageConverter.ToBitmapImage(ImageConverter.CreateBlankImage((int)Math.Floor(ActualWidth), (int)Math.Floor(ActualHeight)));
            }
        }

        private void worker_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            previewImage.Source = _bitmap;
        }
    }
}
