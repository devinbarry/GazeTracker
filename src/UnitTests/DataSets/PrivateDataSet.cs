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
using UnitTests.DataSets;
using log4net;

namespace UnitTests
{
    public abstract class PrivateDataSet : TestDataSet
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

        /// <summary>
        /// Private Data Set
        /// </summary>
        public static readonly PrivateDataSet PUT_FACE_DATABASE = new PutFaceDatabaseDataSet();
        public static readonly PrivateDataSet POLYU_NIR_FACE = new PolyuNirFaceDataSet();
        public static readonly PrivateDataSet CASIA_IRIS_DISTANCE = new CasiaIrisDistanceDataSet();

        private static readonly string PRIVATE_BASE_DIRECTORY = TEST_BASE_DIRECTORY + "/private";

        public static IEnumerable<PrivateDataSet> Values
        {
            get
            {
                yield return PUT_FACE_DATABASE;
                yield return POLYU_NIR_FACE;
                yield return CASIA_IRIS_DISTANCE;
            }
        }

        private readonly string name;

        protected PrivateDataSet( string name )
        {
            this.name = name;
        }

        protected string getName()
        {
            return this.name;
        }

        public string subdirectory
        {
            get
            {
                return name;
            }
        }

        override protected string directory()
        {
            return PRIVATE_BASE_DIRECTORY + "/" + name;
        }

        public override string ToString()
        {
            return name;
        }

    }

}
