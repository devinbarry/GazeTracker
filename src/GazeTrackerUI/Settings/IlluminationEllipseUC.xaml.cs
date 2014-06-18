using System.Windows.Controls;


namespace GazeTrackerUI.SettingsUI
{
	public partial class IlluminationEllipseUC : UserControl
	{
		public IlluminationEllipseUC()
		{
			this.InitializeComponent();
		}

        public IlluminationEllipseUC(int width, int height)
		{
			this.InitializeComponent();
            this.Width = width;
            this.Height = height;
		}
	}
}