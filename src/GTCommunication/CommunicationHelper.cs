using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTCommunication
{
    public class CommunicationHelper
    {
        static public void setPose6D(Pose6D pose, decimal? x, decimal? y, decimal? z, decimal? yaw, decimal? pitch, decimal? roll, decimal? confidence)
        {
            pose.XSpecified = x.HasValue;
            pose.YSpecified = y.HasValue;
            pose.ZSpecified = z.HasValue;
            pose.YawSpecified = yaw.HasValue;
            pose.PitchSpecified = pitch.HasValue;
            pose.RollSpecified = roll.HasValue;
            pose.ConfidenceSpecified = confidence.HasValue;

            if (x.HasValue)
            {
                pose.X = x.Value;
            }
            if (y.HasValue)
            {
                pose.Y = y.Value;
            }
            if (z.HasValue)
            {
                pose.Z = z.Value;
            }
            if (yaw.HasValue)
            {
                pose.Yaw = yaw.Value;
            }
            if (pitch.HasValue)
            {
                pose.Pitch = pitch.Value;
            }
            if (roll.HasValue)
            {
                pose.Roll = roll.Value;
            }
            if (confidence.HasValue)
            {
                pose.Confidence = confidence.Value;
            }
        }

        // Static class, can't be instantiated
        private CommunicationHelper()
        { }

    }
}
