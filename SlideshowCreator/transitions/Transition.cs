using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowCreator
{
    public abstract class Transition
    {
        public int ExecutionTime = 1000; //default: 1s

        public abstract List<Bitmap> Render(Bitmap start, Bitmap end, int frames);
    }
}
