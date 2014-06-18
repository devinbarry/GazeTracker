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

namespace UnitTests.DataSetMediaApi
{
    public class MediaSequence : IMediaSequence
    {
        protected IEnumerable<IMediaFrame> _frames;
        protected string _identifier;

        public MediaSequence( string identifier, IEnumerable<IMediaFrame> mediaFrames )
        {
            _identifier = identifier;
            _frames = mediaFrames;
        }


        protected MediaSequence( string identifier )
        {
            // Only subclasses can create with 1 argument, and they need to create with an idenfitifer
            _identifier = identifier;
        }

        // No-one can create without an identifier
        private MediaSequence()
        { }

        #region IMediaSequence Members

        public IEnumerable<IMediaFrame> getMediaFrames()
        {
            return _frames;
        }

        public string getMediaSequenceIdentifier()
        {
            return _identifier;
        }

        #endregion
    }

}
