using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
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

        /// <summary>
        /// Initialize the applicaton with all keyboard shortcuts
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            RoutedCommand Newcmd = new RoutedCommand();
            Newcmd.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd, MenuItem_Click));
            RoutedCommand Newcmd1 = new RoutedCommand();
            Newcmd1.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
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
            RoutedCommand Newcmd7 = new RoutedCommand();
            Newcmd7.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(Newcmd7, SaveMenu_Click));
        }

        /// <summary>
        /// Loads the MainWindow if the application is double-clicked on a project file
        /// </summary>
        /// <param name="loadFromFile"></param>
        public MainWindow(string loadFromFile)
        {
            _loadFromFile = loadFromFile;
            InitializeComponent();
        }

        /// <summary>
        /// Responsively adjust the window layout
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pictureExplorerPanel.MaxWidth = (e.NewSize.Width - 30) / 1.5;
            pictureExplorerPanel.MinWidth = (e.NewSize.Width - 30) / 4;
        }

        /// <summary>
        /// Responsively adjust the timeline content
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
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
                if (timeline.MainCanvas.ActualWidth < e.NewSize.Width - 8)
                {
                    timeline.MainCanvas.Width = e.NewSize.Width;
                }
            }
        }

        /// <summary>
        /// Exit application
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Finish slideshow click
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            _dataStore = _dataStore ?? new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            ExportVideoSettingsWindow videoSettings = new ExportVideoSettingsWindow(timeline, _dataStore.ExportData);
            videoSettings.ShowDialog();
        }

        /// <summary>
        /// Set timeline to explorers
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void ColumnDefinition_Loaded(object sender, RoutedEventArgs e)
        {
            pictureExplorer.Timeline = timeline;
            musicExplorer.Timeline = timeline;
        }

        /// <summary>
        /// Preview Button click (WIP)
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //timeline.preview();
        }

        /// <summary>
        /// Saves or (Save as if not saved yet) the current window 
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
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

        /// <summary>
        /// Opens the explorer to choose a save location and saves as project file
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            DataStore ds = _dataStore ?? new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            ds.Update();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Slideshow Creator Project|*.ssc;*.SSC";
            sfd.FileName = "MySlideshow.ssc";
            if (sfd.ShowDialog() == true)
            {
                ds.SavingPath = sfd.FileName;
                ds.SaveTo(ds.SavingPath);
                _dataStore = ds;
                statusBar.SavingPath = sfd.FileName;
            }
        }

        /// <summary>
        /// Opens the explorer to open a menu file
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            _dataStore = new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Slideshow Creator Project|*.ssc;*.SSC";
            if (ofd.ShowDialog() == true)
            {
                _dataStore.SavingPath = ofd.FileName;
                statusBar.SavingPath = ofd.FileName;
                _dataStore.LoadFrom(_dataStore.SavingPath);
            }
        }

        /// <summary>
        /// Initializes association and DataStore after startup
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AssociationManager.EnsureAssociationsSet(".ssc", "SSC_PROJECT_FILE", "Slideshow Creator Project", System.Reflection.Assembly.GetExecutingAssembly().Location);
            pictureExplorer.StatusBar = statusBar;
            if (_loadFromFile == null)
                return;
            _dataStore = new DataStore(timeline, statusBar, pictureExplorer, musicExplorer, new ExportData(timeline, statusBar, 1, 10));
            _dataStore.SavingPath = _loadFromFile;
            statusBar.SavingPath = _loadFromFile;
            _loadFromFile = null;
            _dataStore.LoadFrom(_dataStore.SavingPath);
        }

        //Some spare events (should be removed)

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
        private void AddEmptySlide_Click(object sender, RoutedEventArgs e)
        {
            timeline.AddEmptySlide();

        }

        /// <summary>
        /// Creates a new project. Deletes all existing elements
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">args</param>
        private void NewProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            pictureExplorer.Reset();
            musicExplorer.Reset();
            timeline.PictureElements.Clear();
            timeline.MusicElements.Clear();

            //Remove Music End Elements
            List<UIElement> toRemove = new List<UIElement>();
            foreach (UIElement child in timeline.MainCanvas.Children)
            {
                if (child.GetType() == typeof(TlMusikElementEnde))
                {
                    toRemove.Add(child);
                }
            }
            foreach (UIElement element in toRemove)
            {
                timeline.MainCanvas.Children.Remove(element);
            }
            timeline.EndElements.Clear();

            timeline.UpdateDrawings();
        }

        private void AddMusicMenuItem_Click(object sender, RoutedEventArgs e)
        {
            musicExplorer.ShowAddMusicWindow();
        }

        private void AddPicturesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            pictureExplorer.ShowAddPicturesWindow();
        }
    }
}
