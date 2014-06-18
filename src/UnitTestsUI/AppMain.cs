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
using log4net;
using GTCommunication;
using GTHardware.Cameras.FileStream;
using GazeTrackingLibrary.Detection;
using UnitTests.VisualImpl;
using UnitTests;

namespace UnitTestsUI
{
    public class Program
    {
        private static readonly ILog log = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;

        static void Main( string[] args )
        {
            log.Debug( "Unit Test App starting" );

            YamlTest yt = new YamlTest();
            //yt.testBasicYamlOperation();
            //yt.testYamlReadingFromFile();
            //yt.testYamlInterpretationForDataSetPointList();

            JsonTest jt = new JsonTest();
            //jt.testJsonSerialisation();

            // EyeDetectorTest et = new EyeDetectorTest();
            // VisualDisplay vd = new VisualDisplay();
            FileSystemVisualStore vd = new FileSystemVisualStore( UnitTestSettings.MEDIA_DIRECTORY_TEST_OUTPUT, "jpg", 200, 200 );
            EyeDetectorTest et = new EyeDetectorTest( vd );
            et.setUp();
            //et.PutEyesDetectionTest();
            //et.PolyuEyesDetectionTest();
            et.testStdDev();
            et.BinocularDetectionTest();
            et.tearDown();
            // vd.Close();

            XmlStructureTest xt = new XmlStructureTest();
            //xt.testBasicStructure();

            FileStreamCameraTest t1 = new FileStreamCameraTest();
            //t1.SimpleFrameLoadingTest();

            OpenCvExperimentTest t2 = new OpenCvExperimentTest();
            //t2.BasicGrayUsageTest();

            et = null;
            xt = null;
            t1 = null;
            t2 = null;
        }
    }

}
