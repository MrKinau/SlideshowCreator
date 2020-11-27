using System.Runtime.Serialization;

namespace SlideshowCreator
{
    /// <summary>
    /// Data object for all timeline pictue element relevant data
    /// </summary>
    [DataContract]
    class TimelineMusicElementData
    {

        [DataMember] public double StartTime;
        [DataMember] public double EndTime;

        [DataMember] public string MusicPath;

        /// <summary>
        /// Setting up new timeline music element data
        /// </summary>
        /// <param name="startTime">music element start time</param>
        /// <param name="endTime">music element end time</param>
        /// <param name="musicPath">music element thumbnail path</param>
        public TimelineMusicElementData(double startTime, double endTime, string musicPath)
        {
            StartTime = startTime;
            EndTime = endTime;
            MusicPath = musicPath;
        }
    }
}
