using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
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
using System.Xml;
using Microsoft.Win32;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string _loadFromFile;
        private DataStore _dataStore;

        public AssociationSettings AssociationManager = new AssociationSettings();

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(string loadFromFile)
        {
            _loadFromFile = loadFromFile;
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pictureExplorerPanel.MaxWidth = (e.NewSize.Width - 30) / 1.5;
            pictureExplorerPanel.MinWidth = (e.NewSize.Width - 30) / 4;
        }

        private void Timeline_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            { 
                foreach (TimelinePictureElementControl element in timeline.PictureElements)
                {
                    element.UpdateHeight();
                    element.Update();
                }
            }
            else if (e.WidthChanged)
            {
                if (timeline.mainCanvas.ActualWidth < e.NewSize.Width - 8)
                {
                    timeline.mainCanvas.Width = e.NewSize.Width;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PackButton_Click(object sender, RoutedEventArgs e)
        {
            timeline.Pack();
        }

       
        private void LsdButton_Click(object sender, RoutedEventArgs e)
        {
            Random _rnd = new Random();
            foreach (TimelinePictureElementControl element in timeline.PictureElements)
            {
                element.tlElementContent.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)_rnd.Next(256), (byte)_rnd.Next(256), (byte)_rnd.Next(256)));
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            _dataStore = _dataStore ?? new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            ExportVideoSettingsWindow videoSettings = new ExportVideoSettingsWindow(timeline, _dataStore.ExportData);
            videoSettings.ShowDialog();
        }

        private void ColumnDefinition_Loaded(object sender, RoutedEventArgs e)
        {
            pictureExplorer.Timeline = timeline;
            musicExplorer.Timeline = timeline;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //timeline.preview();
        }
        
        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_dataStore == null || _dataStore.SavingPath == null)
            {
                SaveAsMenu_Click(null, null);
                return;
            }

            _dataStore.Update();
            _dataStore.SaveTo(_dataStore.SavingPath);
        }

        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            DataStore ds = _dataStore ?? new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            ds.Update();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Slideshow Creator Project|*.ssc;*.SSC";
            sfd.FileName = "MySlideshow.ssc";
            if (sfd.ShowDialog() == true)
            {
                Title = "Slideshow Creator // " + sfd.FileName;
                ds.SavingPath = sfd.FileName;
                ds.SaveTo(ds.SavingPath);
                _dataStore = ds;
            }
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            _dataStore = new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Slideshow Creator Project|*.ssc;*.SSC";
            if (ofd.ShowDialog() == true)
            {
                _dataStore.SavingPath = ofd.FileName;
                Title = "Slideshow Creator // " + _dataStore.SavingPath;
                _dataStore.LoadFrom(_dataStore.SavingPath);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AssociationManager.EnsureAssociationsSet(".ssc", "SSC_PROJECT_FILE", "Slideshow Creator Project", System.Reflection.Assembly.GetExecutingAssembly().Location);
            pictureExplorer.StatusBar = statusBar;
            if (_loadFromFile == null)
                return;
            _dataStore = new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            _dataStore.SavingPath = _loadFromFile;
            _loadFromFile = null;
            Title = "Slideshow Creator // " + _dataStore.SavingPath;
            _dataStore.LoadFrom(_dataStore.SavingPath);
        }

        private void AddEmptySlide_Click(object sender, RoutedEventArgs e)
        {
            timeline.AddEmptySlide();
        }
    }
}
