using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowCreator
{
    [DataContract]
    class TimelineElementData
    {

        [DataMember] public double StartTime;
        [DataMember] public double EndTime;

        [DataMember] public string Thumbnail;

        public TimelineElementData(double startTime, double endTime, string thumbnail)
        {
            StartTime = startTime;
            EndTime = endTime;
            Thumbnail = thumbnail;
        }
    }
}
