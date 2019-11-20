using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for MusicExplorerControl.xaml
    /// </summary>
    public partial class MusicExplorerControl : UserControl
    {

        private List<string> _musicPaths = new List<string>();
        private TimelineControl _timeline;

        public TimelineControl Timeline
        {
            set { _timeline = value; }
        }

        public MusicExplorerControl()
        {
            InitializeComponent();
        }

        private void AddMusic(string file)
        {
            if (_musicPaths.Contains(file))
                return;
            _musicPaths.Add(file);

            MusicExplorerElementControl musicItem = new MusicExplorerElementControl();
            musicItem.TextBlock.Text = Path.GetFileName(file);
            musicItem.TextBlock.ToolTip = Path.GetFileName(file);
            musicItem.Margin = new Thickness(2);
            musicItem.MouseDoubleClick += OnMusicClick;

            Music_Holder.Children.Add(musicItem);
        }

        private void Add_Music_Click(object sender, RoutedEventArgs e)
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

        private void OnMusicClick(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("TEST");

            if (sender == null)
                return;

            if (sender.GetType() != typeof(MusicExplorerElementControl))
                return;

            int index = Music_Holder.Children.IndexOf((MusicExplorerElementControl)sender);
            _timeline.AddMusicElement(_musicPaths[index]);
        }
    }
}
