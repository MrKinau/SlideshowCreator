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
        //private ImageSource _loadingImg;
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
            //_loadingImg = null;
            _loadingImgCount = 0;
        }

        public void AddImages(string[] fileNames)
        {
            foreach (String file in fileNames)
            {
                
                Picture_ContextMenu NewPic = new Picture_ContextMenu(this, file);
                NewPic.MouseLeftButtonDown += new MouseButtonEventHandler(OnImgClick);
                if (_imgPaths.Contains(file))
                    return;
                _imgPaths.Add(file);
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

       
        private void Add_Img_Click(object sender, RoutedEventArgs e)
        {

           // _loadingImg = null;
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

            if (sender.GetType() != typeof(Picture_ContextMenu))
                return;

            int index = Picture_Holder.Children.IndexOf((Picture_ContextMenu)sender);
            
            _timeline.AddPictureElement(_imgPaths[index]);
        }

        private void loadWorker_work(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < _loadingImgCount; i++)
            {
                //_loadingImg = ImageConverter.ScaleToBitmapImage(new Uri(_imgPaths[i]), 100, 100);
                (sender as BackgroundWorker).ReportProgress(i);
            }
        }

        private void loadWorker_updateImg(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > Picture_Holder.Children.Count - 1)
                return;
            if (StatusBar != null)
                StatusBar.LoadingValue = e.ProgressPercentage;
            //Picture_ContextMenu img = (Picture_ContextMenu)Picture_Holder.Children[e.ProgressPercentage];
            //img.added_Img.Margin = new Thickness(2);
            //img.added_Img.MaxWidth = img.added_Img.MaxHeight = 100;
            //img.added_Img.Source = _loadingImg;
        }

        private void loadWorker_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (StatusBar != null)
                StatusBar.LoadingText = "";
        }
    }
}
