using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UnitTests.VisualApi;
using Emgu.CV.UI;

namespace UnitTestsUI
{
    public partial class VisualDisplay : Form, IVisualDisplay
    {
        private static readonly int MAX_IMAGE_BOXES = 3;

        private string[] _imageBoxNames = new string[ MAX_IMAGE_BOXES ];

        public VisualDisplay()
        {
            InitializeComponent();
            _imageBoxNames[ 0 ] = "";
            _imageBoxNames[ 1 ] = "";
            _imageBoxNames[ 2 ] = "";
            flowPanel1.Dock = DockStyle.Fill;
            flowPanel2.Dock = DockStyle.Fill;
            flowPanel3.Dock = DockStyle.Fill;
            tablePanel1.Dock = DockStyle.Fill;
        }

        #region IVisualDisplay Members

        public void addImage( string imageId, string imageType, Emgu.CV.Image<Emgu.CV.Structure.Gray, byte> image )
        {
            ensureImageBoxNamesUpdated( imageType );
            ImageBox ib = new ImageBox();
            ib.Image = image;
            ib.Width = 100;
            ib.Height = 100;
            ib.SizeMode = PictureBoxSizeMode.Zoom;
            ib.HorizontalScrollBar.Visible = false;
            ib.VerticalScrollBar.Visible = false;
            if ( _imageBoxNames[ 0 ].CompareTo( imageType ) == 0 )
            {
                flowPanel1.Controls.Add( ib );
                return;
            }
            if ( _imageBoxNames[ 1 ].CompareTo( imageType ) == 0 )
            {
                flowPanel2.Controls.Add( ib );
                return;
            }
            if ( _imageBoxNames[ 2 ].CompareTo( imageType ) == 0 )
            {
                flowPanel3.Controls.Add( ib );
            }
            return;
        }

        public void addImage( string imageId, string imageType, Bitmap image )
        {
            ensureImageBoxNamesUpdated( imageType );
            PictureBox ib = new PictureBox();
            ib.Image = image;
            ib.Width = 100;
            ib.Height = 100;
            ib.SizeMode = PictureBoxSizeMode.Zoom;
            //ib.HorizontalScrollBar.Visible = false;
            //ib.VerticalScrollBar.Visible = false;
            if ( _imageBoxNames[ 0 ].CompareTo( imageType ) == 0 )
            {
                flowPanel1.Controls.Add( ib );
                return;
            }
            if ( _imageBoxNames[ 1 ].CompareTo( imageType ) == 0 )
            {
                flowPanel2.Controls.Add( ib );
                return;
            }
            if ( _imageBoxNames[ 2 ].CompareTo( imageType ) == 0 )
            {
                flowPanel3.Controls.Add( ib );
            }
            return;
        }

        public void setVisible( bool visible )
        {
            if ( visible )
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }

        public void refresh()
        {
            this.PerformLayout();
            this.Refresh();
        }

        #endregion

        private void ensureImageBoxNamesUpdated( string imageName )
        {
            int freeIndex = 0;
            for ( int i = 0; i < MAX_IMAGE_BOXES; i++ )
            {
                if ( _imageBoxNames[ i ].CompareTo( "" ) != 0 )
                {
                    freeIndex++;
                    if ( _imageBoxNames[ i ].CompareTo( imageName ) == 0 )
                    {
                        return;
                    }
                }
            }
            if ( freeIndex < MAX_IMAGE_BOXES )
            {
                _imageBoxNames[ freeIndex ] = imageName;
            }
        }

    }
}
