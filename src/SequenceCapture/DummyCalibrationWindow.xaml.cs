using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SequenceCapture
{
    public partial class DummyCalibrationWindow : Window
    {
        public DummyCalibrationWindow()
        {
            InitializeComponent();
        }


        internal void SetSize(System.Drawing.Size size)
        {
            this.WindowStyle = WindowStyle.None;
            calibrationControl.Width = (double) size.Width;
            calibrationControl.Height = (double) size.Height;
            Canvas.SetTop(calibrationControl, 0);
            Canvas.SetLeft(calibrationControl, 0);
            calibrationControl.PointDiameter = 40;
            calibrationControl.NumberOfPoints = 9;

            Show();
            this.WindowState = WindowState.Maximized;
        }
    }
}
