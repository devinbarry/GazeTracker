using System;
using System.Collections.Generic;
using System.Text;
using UnitTests.VisualApi;

namespace UnitTests.VisualImpl
{
    public class FileSystemVisualStore : IVisualDisplay
    {
        static private readonly Emgu.CV.Structure.Gray GRAY_WHITE = new Emgu.CV.Structure.Gray( 255 );
        static private readonly Emgu.CV.Structure.Gray GRAY_BLACK = new Emgu.CV.Structure.Gray( 0 );

        string _path = "./";
        string _imageExtension = "bmp";
        bool _scaleImageBeforeStorage = false;
        int _sizeX = 0;
        int _sizeY = 0;

        public FileSystemVisualStore( string path, string imageExtension )
        {
            _path = path;
            _imageExtension = imageExtension;
            _scaleImageBeforeStorage = false;
            _sizeX = 0;
            _sizeY = 0;
        }

        public FileSystemVisualStore( string path, string imageExtension, int sizeX, int sizeY )
        {
            _path = path;
            _imageExtension = imageExtension;
            _sizeX = sizeX;
            _sizeY = sizeY;
            _scaleImageBeforeStorage = true;
        }

        private string generateFilename( string imageId, string imageType )
        {
            return imageType + "-" + imageId;
        }

        private void tagImageWithResult( string imageType, Emgu.CV.Image<Emgu.CV.Structure.Gray, byte> image )
        {
            if ( imageType.CompareTo( "FOUND_OK" ) == 0 )
            {
                // add a white square 10% of image size
                image.Draw( new System.Drawing.Rectangle( 0, 0, image.Width / 10, image.Height / 10 ), GRAY_WHITE, 0 );
            }
            if ( imageType.CompareTo( "FOUND_WRONG" ) == 0 )
            {
                // add a white square 10% of image size
                image.Draw( new Emgu.CV.Structure.CircleF( new System.Drawing.PointF( 0, 0 ), image.Width / 10 ), GRAY_WHITE, 0 );
            }
            if ( imageType.CompareTo( "NOT_FOUND" ) == 0 )
            {
                // add a white square 10% of image size
                image.Draw( new Emgu.CV.Structure.Cross2DF( new System.Drawing.PointF( image.Width / 20, image.Height / 20 ), image.Width / 10, image.Height / 10 ), GRAY_WHITE, 0 );
            }
        }

        #region IVisualDisplay Members

        public void addImage( string imageId, string imageType, Emgu.CV.Image<Emgu.CV.Structure.Gray, byte> image )
        {
            string fileName = _path + "/" + generateFilename( imageId, imageType ) + "." + _imageExtension;
            if ( _scaleImageBeforeStorage == true )
            {
                Emgu.CV.Image<Emgu.CV.Structure.Gray, byte> scaledImage = image.Resize( _sizeX, _sizeY, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, true );
                tagImageWithResult( imageType, scaledImage );
                scaledImage.Save( fileName );
            }
            else
            {
                tagImageWithResult( imageType, image );
                image.Save( fileName );
            }
        }

        public void setVisible( bool visible )
        {
            // not relevant to file system storage
        }

        public void refresh()
        {
            // not relevant to file system storage
        }

        #endregion
    }
}
