// <copyright>
// ******************************************************
// UnitTests for ITU GazeTracker
// Copyright (C) 2011 Dot Au Pty Limited
// ------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation; either version 3 of the License, 
// or (at your option) any later version.
// This program is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
// General Public License for more details.
// You should have received a copy of the GNU General Public License 
// along with this program; if not, see http://www.gnu.org/licenses/.
// **************************************************************
// </copyright>
// <author>Alastair Jeremy</author>
// <email>gaze@dotaussie.com.au</email>
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using log4net;
using NUnit.Framework;
using UnitTests.DataSetMediaApi;
using UnitTests.DataSetMediaImpl;

namespace UnitTests.DataSets
{
    public class PolyuNirFaceDataSet : PrivateDataSet
    {
        #region Logging Setup
        /// <summary>
        /// Logger for log4net logging
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
        /// <summary>
        /// Indicator that can be used for high speed DEBUG level logging. Note that using this flag will prevent automated reload
        /// of log4net configuration for that log statement during program operation
        /// </summary>
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;
        #endregion

        private static List<IMediaSequence> _mediaSequences = null;

        private static Object obj = new Object();

        public PolyuNirFaceDataSet()
            : base( "polyu-nir-face" )
        {
        }

        public override IEnumerable<IMediaSequence> getMediaSequences()
        {
            lock ( obj )
            {
                if ( _mediaSequences == null )
                {
                    // Create a list of directories to look in for images
                    List<string> imageDirectories = new List<string>();
                    for ( int n = 1; n <= 6; n++ )
                    {
                        imageDirectories.Add( "NIRface" + n );
                    }

                    // Add the media sequences (which will be lazy-loading single frames with just the eyes identified) to the list,
                    // using the text file of known positions in the first directory
                    _mediaSequences = new List<IMediaSequence>();
                    _mediaSequences.AddRange( nirFaceLabelledFeatures( "NIRface1/FaceFP_2.txt", imageDirectories ) );
                }
            }
            return _mediaSequences;
        }

        /// <summary>
        /// Extract all the images we can find that have labelled eyes and also a corresponding image file
        /// </summary>
        /// <param name="truthFilename"></param>
        /// <returns></returns>
        private IEnumerable<IMediaSequence> nirFaceLabelledFeatures( string truthFilename, List<string> imageDirectories )
        {
            long missingFiles = 0;
            string groundTruthDataFile = directory() + "/" + truthFilename;

            // In this case each media sequence in this list will simply contain a single MediaFrame with a null timestamp
            List<IMediaSequence> mediaSequence = new List<IMediaSequence>();

            // create reader & open file
            TextReader tr = new StreamReader( groundTruthDataFile );
            String line = tr.ReadLine();
            while ( line != null )
            {
                // read a line of text
                String[] breakup = line.Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );

                string identifier = breakup[ 0 ].Replace( ".bmp", "" );
                string imgSubdirectory = ( breakup[ 0 ].Split( '_' ) )[ 1 ];
                bool fileFound = false;
                string imgFilename = null;
                foreach ( string imageDirectory in imageDirectories )
                {
                    imgFilename = directory() + "/" + imageDirectory + "/" + imgSubdirectory + "/" + ( breakup[ 0 ].Replace( ".bmp", ".jpg" ) );
                    if ( File.Exists( imgFilename ) )
                    {
                        fileFound = true;
                        break;
                    }
                    imgFilename = directory() + "/" + imageDirectory + "/" + imgSubdirectory + "_M_S/" + ( breakup[ 0 ].Replace( ".bmp", ".jpg" ) );
                    if ( File.Exists( imgFilename ) )
                    {
                        fileFound = true;
                        break;
                    }
                    imgFilename = directory() + "/" + imageDirectory + "/" + imgSubdirectory + "_M_B/" + ( breakup[ 0 ].Replace( ".bmp", ".jpg" ) );
                    if ( File.Exists( imgFilename ) )
                    {
                        fileFound = true;
                        break;
                    }
                }

                if ( fileFound )
                {
                    // Create a container for these two eye features
                    List<ILabelledFeature> features = new List<ILabelledFeature>();

                    // Pull out the point locations for each eye
                    Point e1 = new Point( Convert.ToInt32( breakup[ 1 ] ), Convert.ToInt32( breakup[ 2 ] ) );
                    Point e2 = new Point( Convert.ToInt32( breakup[ 3 ] ), Convert.ToInt32( breakup[ 4 ] ) );
                    if ( e1.X < e2.X )
                    {
                        // e1 is the right eye & e2 is the left eye
                        features.Add( new LabelledFeature( DataSetEnums.FeatureName.RIGHT_EYE_CENTRE, new SinglePointPath( e1 ) ) );
                        features.Add( new LabelledFeature( DataSetEnums.FeatureName.LEFT_EYE_CENTRE, new SinglePointPath( e2 ) ) );
                    }
                    else
                    {
                        features.Add( new LabelledFeature( DataSetEnums.FeatureName.RIGHT_EYE_CENTRE, new SinglePointPath( e2 ) ) );
                        features.Add( new LabelledFeature( DataSetEnums.FeatureName.LEFT_EYE_CENTRE, new SinglePointPath( e1 ) ) );
                    }

                    // This frame has the two eye features from above
                    IMediaFrame mediaFrame = new LazyLoadImageMediaFrame( imgFilename, features );
                    // This mediaFrame sequence only has the one frame in it
                    mediaSequence.Add( new SingleFrameMediaSequence( getName() + "_" + identifier , mediaFrame ) );
                }
                else
                {
                    missingFiles++;
                }
                line = tr.ReadLine();
            }
            // close the stream
            tr.Close();

            Assert.LessOrEqual( missingFiles, 2354, "More than expected number of image files were not found" );
            return mediaSequence;
        }

    }

}
