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
using System.Drawing;
using NUnit.Framework;
using UnitTests;
using log4net;
using UnitTests.DataSets;
using UnitTests.DataSetMediaApi;
using GazeTrackingLibrary.Detection.Eye;
using Emgu.CV.Structure;
using Emgu.CV;
using GTSettings;
using GTCommons.Enum;
using Emgu.CV.UI;
using UnitTests.VisualApi;

namespace GazeTrackingLibrary.Detection
{
    [TestFixture]
    public class EyeDetectorTest
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

        private enum SearchResult
        {
            FOUND_OK,
            FOUND_WRONG,
            NOT_FOUND
        }

        private IVisualDisplay _visualDisplay = null;
        private bool _displayImages = false;
        private MCvFont _font;

        public EyeDetectorTest()
        {
            _visualDisplay = null;
        }

        public EyeDetectorTest( IVisualDisplay visualDisplay )
        {
            _visualDisplay = visualDisplay;
        }

        [TestFixtureSetUp]
        public void setUp()
        {
            if ( ( UnitTestSettings.RENDER_UNIT_TEST_IMAGES ) && ( _visualDisplay != null ) )
            {
                _displayImages = true;
                _visualDisplay.setVisible( true );
                _font = new MCvFont( Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0 );
            }
        }

        [TestFixtureTearDown]
        public void tearDown()
        {
            if ( ( UnitTestSettings.RENDER_UNIT_TEST_IMAGES ) && ( _visualDisplay != null ) )
            {
                _displayImages = true;
                _visualDisplay.setVisible( false );
            }
        }

        [Test]
        public void testStdDev()
        {
            List<double> l = new List<double>( new double[] { 2, 4, 4, 4, 5, 5, 7, 9 } );
            Assert.AreEqual( 2, calculateStdDev( l ) );
        }

        [Test]
        public void PolyuEyesDetectionTest()
        {
            // Can be used for testing to make sure N-Unit is responding properly
            Assert.IsTrue( true, "Hello World" );
            log.Debug( "PolyuNirEyesDetectionTest" );

            // Load the expected values
            Assert.IsTrue( PrivateDataSet.POLYU_NIR_FACE.exists, "Can't find POLYU NIR DATABASE" );

            // Verify expected number of images with labelled eyes [note that at present there are 3282 eyes labelled in the label file, but only 928 corresponding images]
            Assert.AreEqual( 928, countEnumeration( PrivateDataSet.POLYU_NIR_FACE.getMediaSequences() ), "Unexpected number of images with labelled eyes" );
        }

        [Test]
        public void PutEyesDetectionTest()
        {
            // Can be used for testing to make sure N-Unit is responding properly
            Assert.IsTrue( true, "Hello World" );
            log.Debug( "PutEyesDetectionTest" );

            // Load the expected values
            Assert.IsTrue( PrivateDataSet.PUT_FACE_DATABASE.exists, "Can't find PUT FACE DATABASE" );

            // Verify expected number of images with labelled eyes [note that at present there are 3282 eyes labelled in the label file, but only 928 corresponding images]
            Assert.AreEqual( 9876, countEnumeration( PrivateDataSet.PUT_FACE_DATABASE.getMediaSequences() ), "Unexpected number of images with labelled eyes" );

            foreach ( IMediaSequence ms in PrivateDataSet.PUT_FACE_DATABASE.getMediaSequences() )
            {
                foreach ( IMediaFrame mf in ms.getMediaFrames() )
                {
                    Image img = mf.getImage();
                    mf.getLabelledFeature( DataSetEnums.FeatureName.RIGHT_EYE_CENTRE );
                    mf.getLabelledFeature( DataSetEnums.FeatureName.LEFT_EYE_CENTRE );
                }
            }

            // Define function delegate to pass to the directory-walker

            // Call directory walker    
        }

        [Test]
        public void PersonalViperTest()
        {
            // Can be used for testing to make sure N-Unit is responding properly
            Assert.IsTrue( true, "Hello World" );
            log.Debug( "PersonalViperTest" );

            // Load the expected values
            Assert.IsTrue( PrivateDataSet.CASIA_IRIS_DISTANCE.exists, "Can't find CASIA IRIS DISTANCE DATABASE" );

            // Verify expected number of images with labelled eyes [note that at present there are 3282 eyes labelled in the label file, but only 928 corresponding images]
            Assert.AreEqual( 50, countEnumeration( PrivateDataSet.CASIA_IRIS_DISTANCE.getMediaSequences() ), "Unexpected number of images with labelled eyes" );

            foreach ( IMediaSequence ms in PrivateDataSet.CASIA_IRIS_DISTANCE.getMediaSequences() )
            {
                foreach ( IMediaFrame mf in ms.getMediaFrames() )
                {
                    Image img = mf.getImage();
                    mf.getLabelledFeature( DataSetEnums.FeatureName.RIGHT_EYE_CENTRE );
                    mf.getLabelledFeature( DataSetEnums.FeatureName.LEFT_EYE_CENTRE );
                }
            }

            // Define function delegate to pass to the directory-walker

            // Call directory walker    
        }

        private AppendedMediaSequences datasetsOfInterest()
        {
            AppendedMediaSequences allMs = new AppendedMediaSequences( new List<IMediaSequences> { PrivateDataSet.CASIA_IRIS_DISTANCE } );
            // AppendedMediaSequences allMs = new AppendedMediaSequences( new List<IMediaSequences> { PrivateDataSet.PUT_FACE_DATABASE, PrivateDataSet.POLYU_NIR_FACE } );
            return allMs;
        }

        [Test]
        public void BinocularDetectionTest()
        {
            Settings.Instance.Processing.TrackingMode = TrackingModeEnum.Binocular;
            GazeTrackingLibrary.Detection.Eye.Eyetracker et = new GazeTrackingLibrary.Detection.Eye.Eyetracker();
            Assert.IsTrue( et.IsReady );
            List<double> rightEyeProportionalErrors = new List<double>();
            List<double> leftEyeProportionalErrors = new List<double>();
            long countEyesNotFound = 0;
            long countNoEyeCentreLandmarks = 0;
            long countEyesFoundCorrectly = 0;
            long countEyesFoundIncorrectly = 0;
            long imageCounter = 0;

            AppendedMediaSequences allMs = datasetsOfInterest();

            foreach ( IMediaSequence ms in allMs.getMediaSequences() )
            {
                foreach ( IMediaFrame mf in ms.getMediaFrames() )
                {
                    Image img = mf.getImage();
                    imageCounter = imageCounter + 1;
                    ILabelledFeature eyeR = mf.getLabelledFeature( DataSetEnums.FeatureName.RIGHT_EYE_CENTRE );
                    ILabelledFeature eyeL = mf.getLabelledFeature( DataSetEnums.FeatureName.LEFT_EYE_CENTRE );
                    if ( ( eyeR != null ) && ( eyeL != null ) )
                    {
                        Point rightEyeCentre = eyeR.getPath().getPoints()[ 0 ];
                        Point leftEyeCentre = eyeL.getPath().getPoints()[ 0 ];
                        Bitmap bmp = new Bitmap( img );

                        Image<Gray, byte> gray = new Image<Gray, byte>( bmp );
                        TrackData trackData = new TrackData();
                        bool eyesFound = et.DetectEyes( gray, trackData );
                        if ( eyesFound )
                        {
                            // check more things about the location of the eyes that were found
                            // Record the detection information alongside the MediaFrame so that a little later we can compare the distance that the 
                            // landmark eye centre is from the centre of the detected region, and the corresponding distance between the eyes for each sample
                            if ( trackData.RightROI.Contains( leftEyeCentre ) && trackData.LeftROI.Contains( rightEyeCentre ) )
                            {
                                countEyesFoundCorrectly++;
                                renderEyeDetection( imageCounter, bmp, gray, trackData, SearchResult.FOUND_OK );
                            }
                            else
                            {
                                countEyesFoundIncorrectly++;
                                renderEyeDetection( imageCounter, bmp, gray, trackData, SearchResult.FOUND_WRONG );
                            }

                            // Create a measure for the error
                            Point roiCentreLeftEye = new Point( ( trackData.RightROI.Right + trackData.RightROI.Left ) / 2, ( trackData.RightROI.Top + trackData.RightROI.Bottom ) / 2 );
                            Point roiCentreRightEye = new Point( ( trackData.LeftROI.Right + trackData.LeftROI.Left ) / 2, ( trackData.LeftROI.Top + trackData.LeftROI.Bottom ) / 2 );
                            roiCentreRightEye.Offset( -rightEyeCentre.X, -rightEyeCentre.Y );
                            roiCentreLeftEye.Offset( -leftEyeCentre.X, -leftEyeCentre.Y );
                            rightEyeCentre.Offset( -leftEyeCentre.X, -leftEyeCentre.Y );
                            double radialProportionRight = radialDistance( roiCentreRightEye ) / radialDistance( rightEyeCentre );
                            double radialProportionLeft = radialDistance( roiCentreLeftEye ) / radialDistance( rightEyeCentre );
                            rightEyeProportionalErrors.Add( radialProportionRight );
                            leftEyeProportionalErrors.Add( radialProportionLeft );
                        }
                        else
                        {
                            countEyesNotFound++;
                            renderEyeDetection( imageCounter, bmp, gray, trackData, SearchResult.NOT_FOUND );
                        }
                        bmp.Dispose();
                        gray.Dispose();
                    }
                    else
                    {
                        countNoEyeCentreLandmarks++;
                    }
                    img.Dispose();
                }
            }

            double reMean = calculateMean( rightEyeProportionalErrors );
            double leMean = calculateMean( leftEyeProportionalErrors );
            double reStdDev = calculateStdDev( rightEyeProportionalErrors );
            double leStdDev = calculateStdDev( leftEyeProportionalErrors );

            // For combined POLY_U NIR FACE Database & PUT Face Database
            Assert.True( reMean < 0.15D );
            Assert.True( leMean < 0.15D );
            Assert.True( ( double ) countEyesFoundCorrectly / ( ( double ) countEyesFoundCorrectly + ( double ) countEyesFoundIncorrectly + ( double ) countEyesNotFound ) > 0.6 );
            Assert.True( ( double ) countEyesFoundCorrectly / ( ( double ) countEyesFoundCorrectly + ( double ) countEyesFoundIncorrectly ) > 0.74 );
            Assert.AreEqual( 615, countNoEyeCentreLandmarks );

            // For POLY_U NIR FACE Database
            // Only 24.3% success at the moment (POLYU NIR FACE)
            Assert.True( reMean < 0.15D );
            Assert.True( leMean < 0.15D );
            Assert.True( ( double ) countEyesFoundCorrectly / ( ( double ) countEyesFoundCorrectly + ( double ) countEyesFoundIncorrectly + ( double ) countEyesNotFound ) > 0.2 );
            Assert.True( ( double ) countEyesFoundCorrectly / ( ( double ) countEyesFoundCorrectly + ( double ) countEyesFoundIncorrectly ) > 0.85 );
            Assert.AreEqual( 0, countNoEyeCentreLandmarks );

            // For PUT Face Database
            Assert.True( reMean < 0.15D ); // actual is only slightly under this
            Assert.True( leMean < 0.14D ); // actual is only slightly under this
            Assert.True( countEyesFoundCorrectly >= 6014 ); // We get 6014 as baseline
            Assert.True( countEyesFoundIncorrectly <= 2048 ); // We get 2048 as baseline
            Assert.True( countEyesNotFound <= 1199 ); // We get 1199 as baseline
            Assert.True( countNoEyeCentreLandmarks <= 615 ); // We get 615as baseline
            Assert.AreEqual( 615, countNoEyeCentreLandmarks ); // Should not suddenly be getting more landmarks!
        }

        private double calculateStdDev( List<double> values )
        {
            double ret = 0;
            if ( values.Count > 0 )
            {
                double avg = calculateMean( values );
                //Perform the Sum of (value-avg)_2_2      
                double sum2 = 0;
                foreach ( double dd in values )
                {
                    sum2 = sum2 + Math.Pow( ( dd - avg ), 2 );
                }
                //Put it all together      
                ret = Math.Sqrt( ( sum2 ) / ( values.Count ) );
            }
            return ret;
        }

        private double calculateMean( List<double> values )
        {
            double ret = 0;
            if ( values.Count > 0 )
            {
                //Compute the Average      
                foreach ( double dd in values )
                {
                    ret = ret + dd;
                }
                return ret / values.Count;
            }
            return ret;
        }

        private double radialDistance( Point p )
        {
            return Math.Sqrt( ( p.X * p.X ) + ( p.Y * p.Y ) );
        }

        /// <summary>
        /// Cound the number of labelledEyes in the passed enumerator
        /// </summary>
        /// <param name="labelledEyes"></param>
        /// <returns></returns>
        private long countEnumeration( IEnumerable<IMediaSequence> mediaSequences )
        {
            long count = 0;
            foreach ( MediaSequence m in mediaSequences )
            {
                count++;
            }
            return count;
        }

        private void renderEyeDetection( long imageCounter, Bitmap bmp, Image<Gray, byte> gray, TrackData trackData, SearchResult imageType )
        {
            if ( _displayImages )
            {
                if ( imageType.CompareTo( SearchResult.NOT_FOUND ) != 0 )
                {
                    gray.Draw( new Rectangle( trackData.RightROI.Location, trackData.RightROI.Size ), new Gray( 240 ), 4 );
                    gray.Draw( new Rectangle( trackData.LeftROI.Location, trackData.LeftROI.Size ), new Gray( 240 ), 4 );
                }
                gray.Draw( imageType + " : " + imageCounter, ref _font, new Point( 20, 20 ), new Gray( 240 ) );
                _visualDisplay.addImage( imageCounter.ToString( "000000" ), System.Enum.GetName( typeof( SearchResult ), imageType ), gray );
                _visualDisplay.refresh();
            }
        }

    }
}