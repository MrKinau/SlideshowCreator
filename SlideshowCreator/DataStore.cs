using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace SlideshowCreator
{
    [DataContract]
    class DataStore
    {

        private TimelineControl _timeline;
        private PictureExplorerControl _pictureExplorer;
        private MusicExplorerControl _musicExplorer;

        [DataMember] private List<string> _picturePaths;
        [DataMember] private List<string> _musicPaths;
        [DataMember] private List<TimelineElementData> _timelineData;
        [DataMember] private double _tlMarkerPos;
        [DataMember] private double _tlWidth;

        public DataStore(TimelineControl timeline, PictureExplorerControl pictureExplorer, MusicExplorerControl musicExplorer)
        {
            _timeline = timeline;
            _pictureExplorer = pictureExplorer;
            _musicExplorer = musicExplorer;
        }

        public void Update()
        {
            _picturePaths = _pictureExplorer.ImgPaths;
            _musicPaths = _musicExplorer.MusicPaths;
            _timelineData = new List<TimelineElementData>();
            _tlMarkerPos = Canvas.GetLeft(_timeline.tlMarker);
            _tlWidth = _timeline.mainCanvas.ActualWidth;

            foreach (TimelinePictureElementControl element in _timeline.PictureElements)
            {
                _timelineData.Add(new TimelineElementData(element.StartTime, element.EndTime, element.Thumbnail));
            }
        }

        public void LoadFrom(string fileName)
        {
            Mouse.OverrideCursor = Cursors.AppStarting;
            DataContractSerializer serializer = new DataContractSerializer(typeof(DataStore));
            DataStore ds;
            using (Stream s = File.OpenRead(fileName))
            {
                ds = (DataStore)serializer.ReadObject(s);
            }

            _pictureExplorer.Reset();            _musicExplorer.Reset();            _timeline.PictureElements.Clear();

            _timeline.mainCanvas.Width = ds._tlWidth;
            _timeline.mainScrollbar.ScrollToRightEnd();

            foreach (string musicPath in ds._musicPaths)
                _musicExplorer.AddMusic(musicPath);

            foreach (TimelineElementData elData in ds._timelineData)
            {
                _timeline.AddPictureElement(elData.StartTime, elData.EndTime, elData.Thumbnail);
            }            Canvas.SetLeft(_timeline.tlMarker, ds._tlMarkerPos);

            _pictureExplorer.AddImages(ds._picturePaths.ToArray());

            Mouse.OverrideCursor = null;        }

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
