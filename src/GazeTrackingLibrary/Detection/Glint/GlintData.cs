using GazeTrackingLibrary.Utils;
using GTCommons.Enum;

namespace GazeTrackingLibrary.Detection.Glint
{
    public class GlintData
    {
        public GlintData()
        {
            Glints = new GlintConfiguration(10);
        }

        public EyeEnum Eye { get; set; }

        public GlintConfiguration Glints { get; set; }
    }
}