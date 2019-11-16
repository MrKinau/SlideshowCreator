using Microsoft.Win32;
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
    /// Interaction logic for PictureExplorerControl.xaml
    /// </summary>
    public partial class PictureExplorerControl : UserControl
    {

        private List<string> _imgPaths = new List<string>();
        private TimelineControl _timeline;
        private ImageSource _loadingImg;
        private int _loadingImgCount;

        public TimelineControl Timeline
        {
            set { _timeline = value; }
        }

        public PictureExplorerControl()
        {
            InitializeComponent();
        }

        private void Add_Image(string file)
        {
            _imgPaths.Add(file);

            Image newImg = new Image();
            newImg.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/icons/unknownImage.png", UriKind.Absolute));
            newImg.Margin = new Thickness(2);
            newImg.MaxWidth = newImg.MaxHeight = 100;
            newImg.MouseLeftButtonDown += new MouseButtonEventHandler(OnImgClick);

            Picture_Holder.Children.Add(newImg);
        }

        private void Add_Img_Click(object sender, RoutedEventArgs e)
        {
            _loadingImg = null;
            _loadingImgCount = 0;

            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Multiselect = true;
            OpenFile.Title = "Select Picture(s)";
            OpenFile.Filter = "ALL supported Graphics| *.jpeg; *.jpg;*.png;";
            OpenFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (OpenFile.ShowDialog() == true)
            {
                foreach (String file in OpenFile.FileNames)
                {
                    Add_Image(file);
                }
                _loadingImgCount = Picture_Holder.Children.Count;

                BackgroundWorker loadWorker = new BackgroundWorker();
                loadWorker.WorkerReportsProgress = true;
                loadWorker.DoWork += loadWorker_work;
                loadWorker.ProgressChanged += loadWorker_updateImg;
                loadWorker.RunWorkerAsync();
            }
        }

        private void OnImgClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
                return;

            if (sender == null)
                return;

            if (sender.GetType() != typeof(Image))
                return;

            int index = Picture_Holder.Children.IndexOf((Image)sender);
            _timeline.AddElement(_imgPaths[index]);
        }

        private void loadWorker_work(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < _loadingImgCount; i++)
            {
                _loadingImg = ImageConverter.ScaleToBitmapImage(new Uri(_imgPaths[i]), 100, 100);
                (sender as BackgroundWorker).ReportProgress(i);
            }
        }

        private void loadWorker_updateImg(object sender, ProgressChangedEventArgs e)
        {
            Image img = (Image) Picture_Holder.Children[e.ProgressPercentage];
            img.Source = _loadingImg;
        }
    }
}
