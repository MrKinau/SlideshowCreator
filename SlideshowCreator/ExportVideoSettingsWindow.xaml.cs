using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for ExportVideoSettings Window
    /// </summary>
    public partial class ExportVideoSettingsWindow : Window
    {
        private ExportData _exportData;
        private TimelineControl _timeline;

        /// <summary>
        /// Initializes a new window
        /// </summary>
        /// <param name="timeline">current timeline</param>
        /// <param name="exportData">export data to load</param>
        public ExportVideoSettingsWindow(TimelineControl timeline, ExportData exportData)
        {
            InitializeComponent();
            DataContext = _exportData = exportData;
            _timeline = timeline;
        }

        /// <summary>
        /// Opens new explorer to select the export path
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void ChangeSavepath_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sdf = new SaveFileDialog();
            sdf.Title = "Export Slideshow";
            sdf.Filter = "MP4 Video Files|*.mp4;*.MP4";
            sdf.InitialDirectory = System.IO.Path.GetDirectoryName(_exportData.ExportPath);
            sdf.FileName = System.IO.Path.GetFileName(_exportData.ExportPath);

            if (sdf.ShowDialog() == true)
            {
                _exportData.ExportPath = sdf.FileName;
            }
        }

        /// <summary>
        /// Starts the export progress with a new VideoCreator
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Close();
            VideoCreator videoCreator = new VideoCreator(
                _exportData.ExportPath,
                _exportData.GetResolution().Width,
                _exportData.GetResolution().Height,
                _exportData.Bitrate,
                _exportData.FPS,
                _timeline.PictureElements,
                _timeline.MusicElements);
            videoCreator.CreateVideo();
        }
    }

    /// <summary>
    /// Class to store export settings. Using bindings to the window
    /// </summary>
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

        /// <summary>
        /// Initializing new Data object
        /// </summary>
        /// <param name="timeline">current timeline</param>
        /// <param name="statusBar">current statusbar</param>
        /// <param name="defaultResolution">id of the used resolution</param>
        /// <param name="defaultFps">fps number</param>
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
        /// <summary>
        /// Calculates the needed bitrate and sets it
        /// </summary>
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

        /// <summary>
        /// Returns the current resolution
        /// </summary>
        /// <returns>current resolution</returns>
        public Resolution GetResolution()
        {
            return _resolution;
        }

        /// <summary>
        /// updates the binding
        /// </summary>
        /// <param name="name">name of the property</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// Object for the Resolution, containing two ints for width and height
    /// </summary>
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
