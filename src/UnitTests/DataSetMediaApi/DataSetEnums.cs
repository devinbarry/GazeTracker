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
    public class DataSetEnums
    {
        public enum FeatureName
        {
            // UNKNOWN, // TODO: Remove usage of this

            RIGHT_EYE_CENTRE,
            RIGHT_EYE_REGION,
            RIGHT_EYE_OUTLINE_CONTOUR,

            RIGHT_EYEBROW_OUTLINE_CONTOUR,
            RIGHT_EYEBROW_OUTER_CORNER,
            RIGHT_EYEBROW_INNER_CORNER,

            RIGHT_EYELID_OUTER_CORNER,
            RIGHT_EYELID_INNER_CORNER,
            RIGHT_EYELID_TOP,
            RIGHT_EYELID_BOTTOM,

            LEFT_EYE_CENTRE,
            LEFT_EYE_REGION,
            LEFT_EYE_OUTLINE_CONTOUR,

            LEFT_EYEBROW_OUTLINE_CONTOUR,
            LEFT_EYEBROW_OUTER_CORNER,
            LEFT_EYEBROW_INNER_CORNER,

            LEFT_EYELID_OUTER_CORNER,
            LEFT_EYELID_INNER_CORNER,
            LEFT_EYELID_TOP,
            LEFT_EYELID_BOTTOM,

            NOSE_CENTRE,
            NOSE_REGION,
            NOSE_OUTLINE_CONTOUR,

            RIGHT_NOSTRIL_CENTRE,
            RIGHT_NOSTRIL_OUTSIDE,
            RIGHT_NOSTRIL_OUTER_CORNER,

            LEFT_NOSTRIL_CENTRE,
            LEFT_NOSTRIL_OUTSIDE,
            LEFT_NOSTRIL_OUTER_CORNER,

            MOUTH_REGION,
            MOUTH_OUTLINE_OUTER_CONTOUR,
            MOUTH_OUTLINE_INNER_CONTOUR,
            MOUTH_OUTSIDE_RIGHT,
            MOUTH_OUTSIDE_LEFT,
            MOUTH_OUTSIDE_TOP,
            MOUTH_OUTSIDE_BOTTOM,
            MOUTH_INSIDE_TOP,
            MOUTH_INSIDE_BOTTOM,

            LIPS_REGION,

            RIGHT_EAR_BOTTOM,
            LEFT_EAR_BOTTOM,

            FACE_REGION,
            FACE_OUTLINE_CONTOUR,

            FACE_OUTLINE_BOTTOM,
            FACE_OUTLINE_RIGHT,
            FACE_OUTLINE_LEFT,
        }
    }

}
