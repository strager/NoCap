using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NoCap.Destinations {
    public interface IImageDestination {
        void PutImage(Image image, string name, IResultThing result);
    }
}
