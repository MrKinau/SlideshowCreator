using System.Windows;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for ExportVideoProgress Window
    /// </summary>
    public partial class ExportVideoProgressWindow : Window
    {

        private VideoCreator _videoCreator;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="videoCreator"></param>
        public ExportVideoProgressWindow(VideoCreator videoCreator)
        {
            this._videoCreator = videoCreator;
            InitializeComponent();
        }

        /// <summary>
        /// Stops the export progress if cancel button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _videoCreator.stopWorker();
        }
    }
}
