using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace SlideshowCreator
{

    /// <summary>
    /// Serialized class to create project files with all savable content
    /// </summary>
    [DataContract]
    class DataStore
    {

        private TimelineControl _timeline;
        private PictureExplorerControl _pictureExplorer;
        private MusicExplorerControl _musicExplorer;
        private StatusbarControl _statusBar;

        [DataMember(Name ="Pictures")] private List<string> _picturePaths;
        [DataMember(Name = "Music")] private List<string> _musicPaths;
        [DataMember(Name = "TimelineSlides")] private List<TimelinePictureElementData> _timelinePictureData;
        [DataMember(Name = "TimelineMusic")] private List<TimelineMusicElementData> _timelineMusicData;
        [DataMember(Name = "TimelineMarkerPosition")] private double _tlMarkerPos;

        [DataMember] public string SavingPath;      //only set directly
        [DataMember] public ExportData ExportData;

        /// <summary>
        /// Constructor to create a new DataStore for saving
        /// </summary>
        /// <param name="timeline">The current timeline</param>
        /// <param name="statusbar">The current StatusBar</param>
        /// <param name="pictureExplorer">The current picture explorer</param>
        /// <param name="musicExplorer">The current music explorer</param>
        /// <param name="exportData">The Settings of the export window</param>
        public DataStore(TimelineControl timeline, StatusbarControl statusbar, PictureExplorerControl pictureExplorer, MusicExplorerControl musicExplorer, ExportData exportData)
        {
            _timeline = timeline;
            _statusBar = statusbar;
            _pictureExplorer = pictureExplorer;
            _musicExplorer = musicExplorer;
            ExportData = exportData;
        }

        /// <summary>
        /// Updates all variables for serialization
        /// </summary>
        public void Update()
        {
            _picturePaths = _pictureExplorer.ImgPaths;
            _musicPaths = _musicExplorer.MusicPaths;
            _timelinePictureData = new List<TimelinePictureElementData>();
            _timelineMusicData = new List<TimelineMusicElementData>();
            _tlMarkerPos = Canvas.GetLeft(_timeline.tlMarker);

            foreach (TimelinePictureElementControl element in _timeline.PictureElements)
            {
                _timelinePictureData.Add(new TimelinePictureElementData(element.StartTime, element.EndTime, element.Thumbnail, element.Transition));
            }

            foreach (TimelineMusicElementControl element in _timeline.MusicElements)
            {
                _timelineMusicData.Add(new TimelineMusicElementData(element.StartTime, element.EndTime, element.MusicPath));
            }
        }

        /// <summary>
        /// Parses a loadable Datastore xml-schemed file. And sets its value to the current Datastore
        /// </summary>
        /// <param name="fileName">The path of the xml-schmed file</param>
        public void LoadFrom(string fileName)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.AppStarting;
                DataContractSerializer serializer = new DataContractSerializer(typeof(DataStore));
                DataStore ds;
                using (Stream s = File.OpenRead(fileName))
                {
                    ds = (DataStore)serializer.ReadObject(s);
                }

                if (ds.ExportData != null)
                {
                    ExportData = new ExportData(_timeline, _statusBar, ds.ExportData.Resolution, ds.ExportData.FPS);
                    ExportData.Bitrate = ds.ExportData.Bitrate;
                    ExportData.ExportPath = ds.ExportData.ExportPath;
                }

                _pictureExplorer.Reset();
                _musicExplorer.Reset();
                _timeline.PictureElements.Clear();
                _timeline.MusicElements.Clear();

                //Remove Music End Elements
                List<UIElement> toRemove = new List<UIElement>();
                foreach (UIElement child in _timeline.MainCanvas.Children)
                {
                    if (child.GetType() == typeof(TlMusikElementEnde))
                    {
                        toRemove.Add(child);
                    }
                }
                foreach (UIElement element in toRemove)
                {
                    _timeline.MainCanvas.Children.Remove(element);
                }
                _timeline.EndElements.Clear();

                _timeline.MainScrollbar.ScrollToRightEnd();

                foreach (string musicPath in ds._musicPaths)
                    _musicExplorer.AddMusic(musicPath);

                foreach (TimelinePictureElementData elData in ds._timelinePictureData)
                {
                    _timeline.AddPictureElement(elData.StartTime, elData.EndTime, elData.Thumbnail, elData.TransitionID, elData.TransitionExecutionTime, -1);
                }

                foreach (TimelineMusicElementData elData in ds._timelineMusicData)
                {
                    _timeline.AddMusicElement(elData.MusicPath);
                }

                Canvas.SetLeft(_timeline.tlMarker, ds._tlMarkerPos);

                _pictureExplorer.AddImages(ds._picturePaths.ToArray());
            } catch(Exception)
            {
                MessageBox.Show("Error while loading file " + fileName + ". File could not be loaded!", "Error opening project", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// Saves the current Datastore-Object to an xml-schemed file
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveTo(string fileName)
        {
            Mouse.OverrideCursor = Cursors.AppStarting;
            DataContractSerializer serializer = new DataContractSerializer(typeof(DataStore));
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (XmlWriter w = XmlWriter.Create(fileName, settings))
            {
                serializer.WriteObject(w, this);
            }
            Mouse.OverrideCursor = null;
        }

    }
}
