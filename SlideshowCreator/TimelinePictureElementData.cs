using System.Runtime.Serialization;

namespace SlideshowCreator
{
    /// <summary>
    /// Data object for all timeline picture element relevant data
    /// </summary>
    [DataContract]
    class TimelinePictureElementData
    {

        [DataMember] public double StartTime;
        [DataMember] public double EndTime;

        [DataMember] public string Thumbnail;
        [DataMember] public int TransitionID = -1;
        [DataMember] public int TransitionExecutionTime;

        /// <summary>
        /// Setting up new timeline picture element data
        /// </summary>
        /// <param name="startTime">picture element start time</param>
        /// <param name="endTime">picture element end time</param>
        /// <param name="thumbnail">picture element thumbnail path</param>
        /// <param name="transition">picture element transition id</param>
        public TimelinePictureElementData(double startTime, double endTime, string thumbnail, Transition transition)
        {
            StartTime = startTime;
            EndTime = endTime;
            Thumbnail = thumbnail;
            if (transition != null)
            {
                TransitionID = Transition.getID(transition);
                TransitionExecutionTime = transition.ExecutionTime;
            }
        }
    }
}
