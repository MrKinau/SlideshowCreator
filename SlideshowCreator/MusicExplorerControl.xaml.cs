using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for the Music Explorer
    /// </summary>
    public partial class MusicExplorerControl : UserControl
    {
        private TimelineControl _timeline;

        public TimelineControl Timeline { set { _timeline = value; } }

        public List<string> MusicPaths { get; } = new List<string>();

        public MusicExplorerControl()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Resets to no music elements
        /// </summary>
        public void Reset()
        {
            Music_Holder.Children.Clear();
            MusicPaths.Clear();
        }

        /// <summary>
        /// Adds a new Music element to the explorer
        /// </summary>
        /// <param name="file">path of the new music</param>
        public void AddMusic(string file)
        {
            if (MusicPaths.Contains(file))
                return;
            MusicPaths.Add(file);

            MusicExplorerElementControl musicItem = new MusicExplorerElementControl();
            musicItem.TextBlock.Text = Path.GetFileName(file);
            musicItem.TextBlock.ToolTip = Path.GetFileName(file);
            musicItem.Margin = new Thickness(2);
            musicItem.MouseDoubleClick += OnMusicClick;
            Music_Holder.Children.Add(musicItem);
        }

        /// <summary>
        /// Opens a new explorer window to select multiple new music files
        /// </summary>
        public void ShowAddMusicWindow()
        {
            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Multiselect = true;
            OpenFile.Title = "Select Music";
            OpenFile.Filter = "Audio Format| *.mp3; *.wav;*.wma;";
            OpenFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            if (OpenFile.ShowDialog() == true)
            {
                foreach (String file in OpenFile.FileNames)
                {
                    AddMusic(file);
                }
            }
        }

        /// <summary>
        /// Opens a new explorer window to select multiple new music files
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void Add_Music_Click(object sender, RoutedEventArgs e)
        {
            ShowAddMusicWindow();
        }

        /// <summary>
        /// Adds music from the explorer to the timeline on clicking on it
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void OnMusicClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            if (sender.GetType() != typeof(MusicExplorerElementControl))
                return;

            int index = Music_Holder.Children.IndexOf((MusicExplorerElementControl)sender);
            _timeline.AddMusicElement(MusicPaths[index]);
        }
    }
}
