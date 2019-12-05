using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Shapes;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for ExportVideoSettingsWindow.xaml
    /// </summary>
    public partial class ExportVideoSettingsWindow : Window
    {
        private ExportData _exportData;
        private TimelineControl _timeline;

        public ExportVideoSettingsWindow(TimelineControl timeline, ExportData exportData)
        {
            InitializeComponent();
            DataContext = _exportData = exportData;
            _timeline = timeline;
        }

        private void ChangeSavepath_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sdf = new SaveFileDialog();
            sdf.Title = "Export Slideshow";
            sdf.Filter = "All Video Files|*.avi;*.mp4;*.AVI;*.MP4";
            sdf.InitialDirectory = System.IO.Path.GetDirectoryName(_exportData.ExportPath);
            sdf.FileName = System.IO.Path.GetFileName(_exportData.ExportPath);

            if (sdf.ShowDialog() == true)
            {
                _exportData.ExportPath = sdf.FileName;
            }
            //TODO: Add error msg
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Close();
            VideoCreator videoCreator = new VideoCreator(
                _exportData.ExportPath,
                _exportData.GetResolution().Width,
                _exportData.GetResolution().Height,
                _exportData.Bitrate,
                _exportData.FPS,
                _timeline.PictureElements);
            videoCreator.CreateVideo();
        }
    }

    [DataContract]
    public class ExportData : INotifyPropertyChanged
    {
        private static readonly List<Resolution> _validResolutions = new Resolution[]
                {
                    new Resolution(2560, 1440),
                    new Resolution(1920, 1080),
                    new Resolution(1280, 720),
                    new Resolution(858, 480),
                    new Resolution(480, 360),
                    new Resolution(352, 240)
                }.ToList();

        private StatusbarControl _statusBar;
        private TimelineControl _timeline;
        private string _exportPath;
        private Resolution _resolution;
        private int _bitrate;
        private int _fps;


        public event PropertyChangedEventHandler PropertyChanged;

        public ExportData(TimelineControl timeline, StatusbarControl statusBar, int defaultResolution, int defaultFps)
        {
            _resolution = _validResolutions[defaultResolution];
            _timeline = timeline;
            _fps = defaultFps;
            _statusBar = statusBar;
            calcBitrate();
        }

        [DataMember]
        public string ExportPath
        {
            get { return _exportPath ?? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "MySlideshow.mp4"); }
            set
            {
                _exportPath = value;
                OnPropertyChanged();
                if (_statusBar != null)
                    _statusBar.SavingPath = value;
            }
        }

        [DataMember]
        public int Resolution
        {
            get { return _validResolutions.IndexOf(_resolution); }
            set
            {
                _resolution = _validResolutions[value];
                OnPropertyChanged();
                calcBitrate();
            }
        }

        [DataMember]
        public int Bitrate
        {
            get { return _bitrate; }
            set
            {
                _bitrate = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public int FPS
        {
            get { return _fps; }
            set
            {
                _fps = value;
                OnPropertyChanged();
            }
        }

        /*
         * Bitrate calculation from: https://www.ezs3.com/public/What_bitrate_should_I_use_when_encoding_my_video_How_do_I_optimize_my_video_for_the_web.cfm
         */
        private void calcBitrate()
        {
            if (_timeline == null || _timeline.PictureElements.Count <= 0)
                return;
            double timeInSeconds = 0;

            foreach (TimelinePictureElementControl element in _timeline.PictureElements)
            {
                timeInSeconds += (element.EndTime - element.StartTime) / 100.0;
                timeInSeconds += element.Transition == null ? 0 : (element.Transition.ExecutionTime / 1000.0);
            }
            Bitrate = (int)Math.Round(_resolution.Height * _resolution.Width * 5 * 0.07) * (int)timeInSeconds;
        }

        public Resolution GetResolution()
        {
            return _resolution;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    [DataContract]
    public class Resolution
    {
        [DataMember] public int Width { get; set; }
        [DataMember] public int Height { get; set; }

        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
