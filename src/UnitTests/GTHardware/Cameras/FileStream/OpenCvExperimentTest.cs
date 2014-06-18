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
using Emgu.CV;
using Emgu.CV.Structure;
using log4net;
using System.Drawing.Imaging;

namespace GTHardware.Cameras.FileStream
{
    [TestFixture]
    public class OpenCvExperimentTest
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

        [Test]
        public void BasicGrayUsageTest()
        {
            // Can be used for testing to make sure N-Unit is responding properly
            log.Debug( "Hello World" );

            Bitmap b = new Bitmap( 1280, 1024 );
            Image<Gray, byte> gr = new Image<Gray, byte>( 1280, 1024, new Gray( 0.1D ) );
            ImageEventArgs args = new ImageEventArgs( gr );
        }


    }
}