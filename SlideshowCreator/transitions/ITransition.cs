using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowCreator
{
    interface ITransition
    {
        List<Bitmap> Render(Bitmap start, Bitmap end, int frames);
    }
}
