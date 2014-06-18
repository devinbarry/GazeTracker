using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using GazeTrackingLibrary.Utils;
using GazeTrackingLibrary.Network;
using System.ComponentModel;

namespace SequenceCapture
{
    public partial class SequenceCaptureMainWindow : Window
    {
        #region Variables

        private DummyCalibrationWindow calWindow;
        private SequenceData sequenceData;
        private bool isRunning = false;
        private System.Timers.Timer timerCheckCamera;

        #endregion

        #region Constructor

        public SequenceCaptureMainWindow()
        {
            InitializeComponent();
            imageBoxCapturedFrame.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ContentRendered += new EventHandler(Window1_ContentRendered);
            
            // Start transfer service (GTCloud.exe)
            CloudProcess.Instance.Start();

            timerCheckCamera = new Timer(4000);
            timerCheckCamera.Elapsed += new ElapsedEventHandler(timerCheckCamera_Elapsed);
            timerCheckCamera.Start();

            TextBoxUsername.GotFocus += new RoutedEventHandler(TextBoxUsername_GotFocus);

            CheckBoxShareData.Checked += new RoutedEventHandler(CheckBoxShareData_Change);
            CheckBoxShowSyncUI.Checked += new RoutedEventHandler(CheckBoxShowSyncUI_Change);
            CheckBoxShowSyncUI.Unchecked += new RoutedEventHandler(CheckBoxShowSyncUI_Change);

            SetFormData();
        }




        private void SetFormData()
        {
            SequenceData tmp = new SequenceData();
            tmp.LoadLatest();

            TextBoxUsername.Text = tmp.Username;
            TextBoxLens.Text = tmp.Lens;
            TextBoxLensMM.Text = tmp.LensMM;
            TextBoxLensIris.Text = tmp.LensIris;

            if(tmp.IRSourceCount == 0)
               RadioIR0.IsChecked = true;
            if (tmp.IRSourceCount == 1)
                RadioIR1.IsChecked = true;
            if (tmp.IRSourceCount == 2)
                RadioIR2.IsChecked = true;
            if (tmp.IRSourceCount == 3)
                RadioIR3.IsChecked = true;
            if (tmp.IRSourceCount == 4)
                RadioIR4.IsChecked = true;
            if (tmp.IRSourceCount == 5)
                RadioIR5.IsChecked = true;

            switch(tmp.CameraView)
            {
                case SequenceData.CameraViewEnum.Binocular:
                    RadioCameraViewBinocular.IsChecked = true;
                    break;
                case SequenceData.CameraViewEnum.Monocular:
                    RadioCameraViewMonocular.IsChecked = true;
                    break;
                case SequenceData.CameraViewEnum.Headmounted:
                    RadioCameraViewHeadmounted.IsChecked = true;
                    break;
            }
        }

        #endregion

        #region Initialize

        private void Window1_ContentRendered(object sender, EventArgs e)
        {
            if (GTHardware.Camera.Instance.DeviceType == GTHardware.Camera.DeviceTypeEnum.DirectShow
                && GTHardware.Camera.Instance.Width == 0)
                GTHardware.Camera.Instance.SetDirectShowCamera(0, 1);

            if (GTHardware.Camera.Instance.DeviceType != GTHardware.Camera.DeviceTypeEnum.DirectShow)
                GTHardware.Camera.Instance.Device.Start();

            GTHardware.Camera.Instance.Device.OnImage += new EventHandler<GTHardware.Cameras.ImageEventArgs>(Device_OnImage);

            if (TextBoxDevice.Text.Length < 1)
                TextBoxDevice.Text = GTHardware.Camera.Instance.Device.Name;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // We need to do this in order to obtain the window handle used for some cameras (frame msg)
            base.OnSourceInitialized(e);
            // GTHardware.Camera.Instance.HwndSource = PresentationSource.FromVisual(this) as HwndSource;
        }

        private void timerCheckCamera_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (imageBoxCapturedFrame.Image == null || imageBoxCapturedFrame.Image.Size.Height == 0)
            {
                timerCheckCamera.Stop();
                MessageBox.Show("Problem initializing camera, no images are streamed from the device.");
            }
        }

        private void TextBoxUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxUsername.Text == "username")
                TextBoxUsername.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            GTHardware.Camera.Instance.Device.Stop();
            //GTHardware.Camera.Instance.Device.Cleanup();

            Environment.Exit(Environment.ExitCode);
        }

        #endregion

        #region Private methods

        private void Device_OnImage(object sender, GTHardware.Cameras.ImageEventArgs e)
        {
            // dont render while capturing
            if (isRunning == false) 
                imageBoxCapturedFrame.Image = e.Image;

            // Store data when displaying points
            if (isRunning && calWindow != null && calWindow.calibrationControl != null)
            {
                // Get location of calibration point
                double x = 0;
                double y = 0;

                // Access canvas location running on different thread by dispatcher
                calWindow.calibrationControl.Dispatcher.BeginInvoke
                (
                    DispatcherPriority.Render,
                    new Action(
                        delegate
                        {
                            x = Math.Round(Canvas.GetTop(calWindow.calibrationControl.CurrentPoint), 0);
                            y = Math.Round(Canvas.GetLeft(calWindow.calibrationControl.CurrentPoint), 0);
                        })
                );

                // Store image and point position
                sequenceData.AddImage(e.Image.Copy(), new Point(x, y));
            }
        }

        private void RunCapture(object sender, RoutedEventArgs e)
        {
            // "Fake" calibration window that animates points
            calWindow = new DummyCalibrationWindow();

            // Set parameters
            calWindow.SetSize(ScreenParameters.PrimaryResolution);
            calWindow.calibrationControl.PointTransitionDuration = 1000;
            calWindow.calibrationControl.PointDuration = 1000;
            calWindow.calibrationControl.PointDiameter = 50;

            // Register for event
            calWindow.calibrationControl.OnPointStart += new RoutedEventHandler(calibrationControl_OnPointStart);
            calWindow.calibrationControl.OnCalibrationEnd += new RoutedEventHandler(calibrationControl_OnCalibrationEnd);

            // Stor data in SequenceData object
            sequenceData = new SequenceData();

            // Set sequnce info
            SetSequenceInfo(sequenceData);

            // Start animation, when the first point has been display, OnPoint event is occurs and we start storing images
            calWindow.calibrationControl.Start();
        }

        private void SetSequenceInfo(SequenceData sd)
        {
            // Keep it simple, no data binding, just dump the data into the new session obj

            sd.Username = TextBoxUsername.Text;
            sd.DeviceName = TextBoxDevice.Text;

            if(RadioIR0.IsChecked.Value)
                sd.IRSourceCount = 0;
            if (RadioIR1.IsChecked.Value)
                sd.IRSourceCount = 1;
            if (RadioIR2.IsChecked.Value)
                sd.IRSourceCount = 2;
            if (RadioIR3.IsChecked.Value)
                sd.IRSourceCount = 3;
            if (RadioIR4.IsChecked.Value)
                sd.IRSourceCount = 4;
            if (RadioIR5.IsChecked.Value)
                sd.IRSourceCount = 5;

            sd.Lens = TextBoxLens.Text;
            sd.LensMM = TextBoxLensMM.Text;
            sd.LensIris = TextBoxLensIris.Text;

            if (RadioCameraViewBinocular.IsChecked.Value)
                sd.CameraView = SequenceData.CameraViewEnum.Binocular;
            if (RadioCameraViewMonocular.IsChecked.Value)
                sd.CameraView = SequenceData.CameraViewEnum.Monocular;
            if (RadioCameraViewHeadmounted.IsChecked.Value)
                sd.CameraView = SequenceData.CameraViewEnum.Headmounted;

            sd.Notes = TextBoxNotes.Text;
        }

        private void calibrationControl_OnPointStart(object sender, RoutedEventArgs e)
        {
            isRunning = true;
        }

        private void calibrationControl_OnCalibrationEnd(object sender, RoutedEventArgs e)
        {
            // Close window
            if(calWindow != null)
                calWindow.Close();

            // Stop storing images, start display preview
            isRunning = false;

            // progress save images
            progressBarSaveImages.Value = 0;
            GridSaveImagesProgress.Visibility = Visibility.Visible;
            BtnRun.Visibility = Visibility.Collapsed;

            // Run worker to save images
            BackgroundWorker workerSaveImages = new BackgroundWorker();
            workerSaveImages.WorkerReportsProgress = true;
            workerSaveImages.ProgressChanged += new ProgressChangedEventHandler(workerSaveImages_ProgressChanged);
            workerSaveImages.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerSaveImages_RunWorkerCompleted);

            // Save session information
            sequenceData.Save(workerSaveImages);
        }

        private void workerSaveImages_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // update progressbar indicator
            progressBarSaveImages.Value = e.ProgressPercentage;
        }

        private void workerSaveImages_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GridSaveImagesProgress.Visibility = Visibility.Collapsed;
            BtnRun.Visibility = Visibility.Visible;

            // Upload GT Cloud Data Storage 
            if (CheckBoxShareData.IsChecked.Value)
            {
                if (CloudProcess.Instance.IsCloudProcessRunning)
                    CloudClient.Instance.Transfer(sequenceData.ID, sequenceData.GetFolder(), CloudClient.TransferType.Sequence);
                else
                    MessageBox.Show("GTCloud service is not running, unable to store data.");
            }
        }


        #region UI Methods

        private void CheckBoxShareData_Change(object sender, RoutedEventArgs e)
        {
            if (CloudProcess.Instance.IsCloudProcessRunning == false)
                AskToRestartService();

            if (CheckBoxShareData.IsChecked.Value)
                GridShowSyncUI.Visibility = Visibility.Visible;
            else
                GridShowSyncUI.Visibility = Visibility.Collapsed;
        }

        private void CheckBoxShowSyncUI_Change(object sender, RoutedEventArgs e)
        {
            if (CloudProcess.Instance.IsCloudProcessRunning == false)
                AskToRestartService();

            if (CheckBoxShowSyncUI.IsChecked.Value)
                CloudClient.Instance.ShowSharingUI();
            else
                CloudClient.Instance.HideSharingUI();
        }

        private void AskToRestartService()
        {
            if (MessageBox.Show("Restart the GT Cloud service?", "Service not running", System.Windows.MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                CloudProcess.Instance.Start();
        }

        #endregion

        #endregion

        #region Window managment

        private void AppMinimize(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        private void AppClose(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        #endregion
    }
}
