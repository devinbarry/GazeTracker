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

namespace GTCommunication
{
    [TestFixture]
    public class XmlStructureTest
    {

        private static readonly ILog log = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;

        [Test]
        public void testBasicStructure()
        {
            Being b = new Being();
            b.Ident = "ALASTAIR";
            b.RightEye = new BeingEye();
            b.RightEye.EyeState = EyeState.FIXATION;
            b.RightEye.GazeVector = new Pose6D();
            CommunicationHelper.setPose6D( b.RightEye.GazeVector, 23.5678549M, 92.64654864M, null, 45.765234560M, 60.34444124365M, null, 0.9M );

            GazeTrackData gt = new GazeTrackData();
            gt.Timestamp = DateTime.UtcNow;
            gt.SequenceNumber = 0;
            System.Collections.Generic.List<Being> beingList = new System.Collections.Generic.List<Being>();
            beingList.Add( b );
            gt.Beings = beingList.ToArray();

            System.Collections.Generic.List<GazeTrackData> gazeTrackDataList = new System.Collections.Generic.List<GazeTrackData>();
            gazeTrackDataList.Add( gt );

            GazeTrackDataSet gtd = new GazeTrackDataSet();
            gtd.GazeTrackData = gazeTrackDataList.ToArray();

            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer( gtd.GetType() );
            TextWriter writer = new SpecificEncodingStringWriter( Encoding.UTF8 );
            XmlWriter xmlWriter = XmlWriter.Create( writer );
            x.Serialize( xmlWriter, gtd );
            Console.WriteLine( "XML:" );
            Console.WriteLine( writer.ToString() );
            Console.WriteLine( "Length: " + writer.ToString().Length );
            log.Debug( "Generated XML" );
            log.Debug( writer.ToString() );
            log.Debug( "Generated XML Length: " + writer.ToString().Length );
        }

    }
}