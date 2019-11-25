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

        public Settings SCSettings = new Settings();

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
            Console.WriteLine("Size changed: " + e.NewSize.Height + "/" + e.NewSize.Width);
        }

        private void Timeline_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            { 
                foreach (TimelinePictureElementControl element in timeline.PictureElements)
                {
                    element.updateHeight();
                    element.update();
                }
            }
            else if (e.WidthChanged)
            {
                if (timeline.mainCanvas.ActualWidth < e.NewSize.Width - 8)
                {
                    timeline.mainCanvas.Width = e.NewSize.Width;
                }
            }
            Console.WriteLine("Size changed: " + e.NewSize.Height + "/" + e.NewSize.Width);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PackButton_Click(object sender, RoutedEventArgs e)
        {
            timeline.Pack();
        }

        /* private void Add_Music_Click(object sender, RoutedEventArgs e)
         {
             OpenFileDialog OpenFile = new OpenFileDialog();
             OpenFile.Multiselect = true;
             OpenFile.Title = "Select Picture(s)";
             OpenFile.Filter = "Audio Format| *.mp3; *.wav;*.wma;";
             if (OpenFile.ShowDialog() == true)
             {
                 foreach (String file in OpenFile.FileNames)
                 {
                     Add_Music(file);
                 }
             }
         }*/
        /* private void Add_Music(string file)
         {
             MediaPlayer player = new MediaPlayer();
             player.Open(new Uri(file));
             Music_Holder.Children.Add(player);
         }*/

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

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            if (SCSettings.SavingPath == null)
            {
                SaveAsMenu_Click(null, null);
                return;
            }

            DataStore ds = _dataStore ?? new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            ds.Update();
            ds.SaveTo(SCSettings.SavingPath);
            _dataStore = ds;
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
                SCSettings.SavingPath = sfd.FileName;
                Title = "Slideshow Creator // " + SCSettings.SavingPath;
                ds.SaveTo(SCSettings.SavingPath);
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
                SCSettings.SavingPath = ofd.FileName;
                Title = "Slideshow Creator // " + SCSettings.SavingPath;
                _dataStore.LoadFrom(SCSettings.SavingPath);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SCSettings.EnsureAssociationsSet(".ssc", "SSC_PROJECT_FILE", "Slideshow Creator Project", System.Reflection.Assembly.GetExecutingAssembly().Location);
            pictureExplorer.StatusBar = statusBar;
            if (_loadFromFile == null)
                return;
            _dataStore = new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            SCSettings.SavingPath = _loadFromFile;
            _loadFromFile = null;
            Title = "Slideshow Creator // " + SCSettings.SavingPath;
            _dataStore.LoadFrom(SCSettings.SavingPath);
        }
    }
}
