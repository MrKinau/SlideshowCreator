using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for PictureExplorer
    /// </summary>
    public partial class PictureExplorerControl : UserControl
    {
        private TimelineControl _timeline;
        private ImageSource _loadingImg;
        private int _loadingImgCount;
        private int _loadingImgStart;
        private BackgroundWorker _loadWorker;

        public StatusbarControl StatusBar;

        public TimelineControl Timeline { set { _timeline = value; } }
        public List<string> ImgPaths { get; } = new List<string>();

        public PictureExplorerControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Deletes all content
        /// </summary>
        public void Reset()
        {
            if (_loadWorker != null && _loadWorker.IsBusy)
                _loadWorker.CancelAsync();
            Picture_Holder.Children.Clear();
            ImgPaths.Clear();
            _loadingImg = null;
            _loadingImgCount = 0;
        }

        /// <summary>
        /// Add images to the explorer
        /// </summary>
        /// <param name="fileNames"></param>
        public void AddImages(string[] fileNames)
        {
            _loadingImgStart = Picture_Holder.Children.Count;
            foreach (String file in fileNames)
            {
                Console.WriteLine(file);
                AddImage(file);
            }
            _loadingImgCount = Picture_Holder.Children.Count;

            if (StatusBar != null)
                StatusBar.AddLoadingProgress("Loading images...", _loadingImgCount);

            _loadWorker = new BackgroundWorker();
            _loadWorker.WorkerReportsProgress = true;
            _loadWorker.WorkerSupportsCancellation = true;
            _loadWorker.DoWork += loadWorker_work;
            _loadWorker.ProgressChanged += loadWorker_updateImg;
            _loadWorker.RunWorkerCompleted += loadWorker_completed;
            _loadWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Open Explorer to open images
        /// </summary>
        public void ShowAddPicturesWindow()
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

        /// <summary>
        /// Add loading image to the explorer
        /// </summary>
        /// <param name="file">path of the loading image</param>
        private void AddImage(string file)
        {
            if (ImgPaths.Contains(file))
                return;
            ImgPaths.Add(file);

            Image newImg = new Image();
            newImg.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/icons/unknownImage.png", UriKind.Absolute));
            newImg.Margin = new Thickness(12);
            newImg.MaxWidth = newImg.MaxHeight = 80;
            newImg.MouseLeftButtonDown += new MouseButtonEventHandler(OnImgClick);

            Picture_Holder.Children.Add(newImg);
        }

        /// <summary>
        /// Open Explorer to open images
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void Add_Img_Click(object sender, RoutedEventArgs e)
        {
            ShowAddPicturesWindow();
        }

        /// <summary>
        /// Drag and drop or double click image to add it to timeline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnImgClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                //Drag img
                int index = Picture_Holder.Children.IndexOf((Image)sender);
                DataObject dataObj = new DataObject(ImgPaths[index]);
                DragDrop.DoDragDrop(sender as Image, dataObj, DragDropEffects.Copy);
            }
            else
            {
                //Double click img
                if (sender == null)
                    return;

                if (sender.GetType() != typeof(Image))
                    return;

                int index = Picture_Holder.Children.IndexOf((Image)sender);
                _timeline.AddPictureElement(ImgPaths[index]);
            }
        }

        /// <summary>
        /// Adding images slowly through a worker to not-freeze the UI and overflow the RAM
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void loadWorker_work(object sender, DoWorkEventArgs e)
        {
            for (int i = _loadingImgStart; i < _loadingImgCount; i++)
            {
                while (_loadingImg != null);    //Wait for screen update
                if (File.Exists(ImgPaths[i]))
                    _loadingImg = ImageConverter.ScaleToBitmapImage(new Uri(ImgPaths[i]), 100, 100);
                else
                    _loadingImg = null;
                (sender as BackgroundWorker).ReportProgress(i);
            }
        }

        /// <summary>
        /// Setting image, updating statusbar
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
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

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "Edit";
            menuItem.Click += delegate
            {
                Edit_Picture Edit = new Edit_Picture();
                int index = Picture_Holder.Children.IndexOf(img);

                Edit.Show();
                Edit.set_source(ImgPaths[index],this);
            };
            contextMenu.Items.Add(menuItem);
            contextMenu.Background = Application.Current.FindResource("SC_BG_COLOR") as SolidColorBrush;
            MenuItem delete = new MenuItem();
            delete.Header = "Delete";
            delete.Click += delegate
            {
                int index = Picture_Holder.Children.IndexOf(img);
                Picture_Holder.Children.Remove(img);
                ImgPaths.Remove(ImgPaths[index]);
            };
            contextMenu.Items.Add(delete);
            img.ContextMenu = contextMenu;
            _loadingImg = null;
        }

        /// <summary>
        /// After completion: Reset statusbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadWorker_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (StatusBar != null)
                StatusBar.LoadingText = "";
        }
    }
}
