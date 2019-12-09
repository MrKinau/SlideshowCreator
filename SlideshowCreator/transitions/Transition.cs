using Accord.Video.FFMPEG;
using SlideshowCreator.transitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowCreator
{
    public abstract class Transition
    {
        public int ExecutionTime = 1000; //default: 1s

        public abstract int Render(Bitmap start, Bitmap end, int frames, VideoFileWriter writer, BackgroundWorker progressReporter, int currFrame);

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
