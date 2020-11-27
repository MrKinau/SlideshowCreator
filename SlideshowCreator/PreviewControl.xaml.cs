using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for Preview
    /// </summary>
    public partial class PreviewControl : UserControl
    {

        private BitmapImage _bitmap;
        private string _imgSrc;

        public PreviewControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Updates the preview image async. Without freezing UI
        /// </summary>
        /// <param name="newImg">new image to be displayed</param>
        public void UpdateImageAsync(string newImg)
        {
            UpdateImageAsync(newImg, false);
        }

        /// <summary>
        /// Updates the preview image async. Without freezing UI
        /// </summary>
        /// <param name="newImg">new image to be displayed</param>
        /// <param name="force">overrides anyway</param>
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

        /// <summary>
        /// Worker setting image
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void worker_work(object sender, DoWorkEventArgs e)
        {
            if (ActualWidth > 0 && ActualHeight > 0)
            {
                if (_imgSrc != null)
                    _bitmap = ImageConverter.ScaleToBitmapImage(new Uri(_imgSrc), (int)Math.Floor(ActualWidth), (int)Math.Floor(ActualHeight - 25));
                else
                    _bitmap = ImageConverter.ToBitmapImage(ImageConverter.CreateBlankImage((int)Math.Floor(ActualWidth), (int)Math.Floor(ActualHeight - 25)));
            }
        }

        /// <summary>
        /// Sets the source
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void worker_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            PreviewImage.Source = _bitmap;
        }
    }
}
