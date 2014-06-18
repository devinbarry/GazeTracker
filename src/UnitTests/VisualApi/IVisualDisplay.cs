using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace UnitTests.VisualApi
{
    public interface IVisualDisplay
    {
        void addImage( string imageId, String imageType, Image<Gray, byte> image );

        void setVisible( bool visible );

        void refresh();
    }
}
