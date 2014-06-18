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

namespace GazeTrackingLibrary.Utils
{
    [TestFixture]
    public class OperationsTest
    {
        [Test]
        public void CalculateAngleDegreesTest()
        {
            // Can be used for testing to make sure N-Unit is responding properly
            Assert.IsTrue( true, "Hello World" );

            // Check angle between two simple points
            GTPoint gtPoint1 = new GTPoint( 1.0D, 1.0D );
            GTPoint gtPoint2 = new GTPoint( 2.0D, 2.0D );
            Assert.AreEqual( 45.0D, Operations.CalculateAngle( gtPoint1, gtPoint2 ), UnitTestSettings.DOUBLE_COMPARISON_TOLERANCE );

            // Now create the same GTPoints via two System.Drawing.Point objects and recheck the angles found
            System.Drawing.Point drawingPoint1 = new System.Drawing.Point( 1, 1 );
            System.Drawing.Point drawingPoint2 = new System.Drawing.Point( 2, 2 );
            Assert.AreEqual( 45.0D, Operations.CalculateAngle( drawingPoint1, drawingPoint2 ), UnitTestSettings.DOUBLE_COMPARISON_TOLERANCE );

            // Now create the same GTPoints via two System.Windows.Point objects and recheck the angles found
            System.Windows.Point windowsP1 = new System.Windows.Point( 1, 1 );
            System.Windows.Point windowsP2 = new System.Windows.Point( 2, 2 );
            Assert.AreEqual( 45.0D, Operations.CalculateAngle( windowsP1, windowsP2 ), UnitTestSettings.DOUBLE_COMPARISON_TOLERANCE );

        }

    }
}