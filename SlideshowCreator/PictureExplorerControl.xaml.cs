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
        private BackgroundWorker loadWorker;

        public StatusbarControl StatusBar;

        public TimelineControl Timeline { set { _timeline = value; } }
        public List<string> ImgPaths { get { return _imgPaths; } }

        public PictureExplorerControl()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            if (loadWorker != null && loadWorker.IsBusy)
                loadWorker.CancelAsync();
            Picture_Holder.Children.Clear();
            _imgPaths.Clear();
            _loadingImg = null;
            _loadingImgCount = 0;
        }

        public void AddImages(string[] fileNames)
        {
            foreach (String file in fileNames)
            {
                Add_Image(file);
            }
            _loadingImgCount = Picture_Holder.Children.Count;

            if (StatusBar != null)
                StatusBar.AddLoadingProgress("Loading images...", _loadingImgCount);

            loadWorker = new BackgroundWorker();
            loadWorker.WorkerReportsProgress = true;
            loadWorker.WorkerSupportsCancellation = true;
            loadWorker.DoWork += loadWorker_work;
            loadWorker.ProgressChanged += loadWorker_updateImg;
            loadWorker.RunWorkerCompleted += loadWorker_completed;
            loadWorker.RunWorkerAsync();
        }

        private void Add_Image(string file)
        {
            if (_imgPaths.Contains(file))
                return;
            _imgPaths.Add(file);

            Image newImg = new Image();
            newImg.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/icons/unknownImage.png", UriKind.Absolute));
            newImg.Margin = new Thickness(12);
            newImg.MaxWidth = newImg.MaxHeight = 80;
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
                AddImages(OpenFile.FileNames);
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
            _timeline.AddPictureElement(_imgPaths[index]);
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
            if (e.ProgressPercentage > Picture_Holder.Children.Count - 1)
                return;
            if (StatusBar != null)
                StatusBar.LoadingValue = e.ProgressPercentage;
            Image img = (Image)Picture_Holder.Children[e.ProgressPercentage];
            img.Margin = new Thickness(2);
            img.MaxWidth = img.MaxHeight = 100;
            img.Source = _loadingImg;
        }

        private void loadWorker_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (StatusBar != null)
                StatusBar.LoadingText = "";
        }
    }
}
