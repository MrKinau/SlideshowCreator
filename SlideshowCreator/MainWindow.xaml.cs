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
            RoutedCommand Newcmd = new RoutedCommand();
            Newcmd.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd, MenuItem_Click));
            RoutedCommand Newcmd1 = new RoutedCommand();
            Newcmd1.InputGestures.Add(new KeyGesture(Key.V, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd1, SaveAsMenu_Click));
            RoutedCommand Newcmd2 = new RoutedCommand();
            Newcmd2.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd2, Edit_Transition_Click));
            RoutedCommand Newcmd3 = new RoutedCommand();
            Newcmd3.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd3, Edit_Picture_Click));
            RoutedCommand Newcmd4 = new RoutedCommand();
            Newcmd4.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd4, Undo_Click));
            RoutedCommand Newcmd5 = new RoutedCommand();
            Newcmd5.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd5, Redo_Click));
            RoutedCommand Newcmd6 = new RoutedCommand();
            Newcmd6.InputGestures.Add(new KeyGesture(Key.F1));
            CommandBindings.Add(new CommandBinding(Newcmd6, Help_Click));


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

        private void Edit_Transition_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Edit_Picture_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
