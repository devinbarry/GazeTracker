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
using System.Xml;
using System.IO;
using fastJSON;

namespace GTCommunication
{
    [TestFixture]
    public class JsonTest
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
        public void testJsonSerialisation()
        {
            JSON json = JSON.Instance;

            GazeTrackDataSet ds = new GazeTrackDataSet();
            ds.GazeTrackData = new GazeTrackData[1];
            ds.GazeTrackData[0] = new GazeTrackData();
            ds.GazeTrackData[0].SequenceNumber = 1;
            ds.GazeTrackData[0].Beings = new Being[1];
            ds.GazeTrackData[0].Beings[0] = new Being();
            ds.GazeTrackData[0].Beings[0].Ident = "Person1";
            ds.GazeTrackData[0].Beings[0].LeftEye = new BeingEye();
            ds.GazeTrackData[0].Beings[0].LeftEye.GazeVector = new Pose6D();
            ds.GazeTrackData[0].Beings[0].LeftEye.GazeVector.X = 55;
            ds.GazeTrackData[0].Beings[0].LeftEye.GazeVector.XSpecified = true;
            ds.GazeTrackData[0].Beings[0].LeftEye.GazeVector.YSpecified = false;
            ds.GazeTrackData[0].Beings[0].LeftEye.GazeVector.ZSpecified = false;

            String s = json.ToJSON(ds, false, true, true, false);

            GazeTrackDataSet dsAfter = json.ToObject<GazeTrackDataSet>(s);

            Assert.AreEqual(ds.GazeTrackData[0].SequenceNumber, dsAfter.GazeTrackData[0].SequenceNumber);
        }


    }
}