using Accord.Video.FFMPEG;
using SlideshowCreator.transitions;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace SlideshowCreator
{

    [DataContract]
    public abstract class Transition
    {
        [DataMember] public int ExecutionTime = 1000; //default: 1s

        [DataMember] public int TransitionID
        {
            get { return getID(this); }
        }

        /// <summary>
        /// The function which is called by the creating video stream to add the transition images to it
        /// </summary>
        /// <param name="start">Starting bitmap</param>
        /// <param name="end">Ending bitmap</param>
        /// <param name="frames">The framecount for the transition</param>
        /// <param name="writer">The VideoWriterStream</param>
        /// <param name="progressReporter">A background worker which should get progress reports</param>
        /// <param name="currFrame">The currently rendered frame as int (for the progress worker calculation)</param>
        /// <returns>the current frame count</returns>
        public abstract int Render(Bitmap start, Bitmap end, int frames, VideoFileWriter writer, BackgroundWorker progressReporter, int currFrame);

        /// <summary>
        /// Returns the ID of the given transition
        /// </summary>
        /// <param name="t">given transition</param>
        /// <returns>ID of the given transition</returns>
        public static int getID(Transition t)
        {
            if (t == null)
                return 0;

            if (t.GetType() == typeof(FadeTransition))
                return 1;
            else if (t.GetType() == typeof(PushTransition))
                return 2;
            else if (t.GetType() == typeof(SlideTransition))
                return 3;
            else
                return 0;
        }

        /// <summary>
        /// Returns a new Transition object of the given transition with the given ID
        /// </summary>
        /// <param name="tId">the given id</param>
        /// <returns>new object of the transition with the id</returns>
        public static Transition getByID(int tId)
        {
            switch(tId)
            {
                case 1: return new FadeTransition();
                case 2: return new PushTransition();
                case 3: return new SlideTransition();
                default: return null;
            }
        }
    }
}
