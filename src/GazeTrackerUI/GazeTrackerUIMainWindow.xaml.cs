// <copyright file="Tracker.cs" company="ITU">
// ******************************************************
// GazeTrackingLibrary for ITU GazeTracker
// Copyright (C) 2010 Martin Tall. All rights reserved. 
// ------------------------------------------------------------------------
// We have a dual licence, open source (GPLv3) for individuals - licence for commercial ventures.
// You may not use or distribute any part of this software in a commercial product. Contact us to arrange a licence. 
// We accept no responsibility or liability.
// </copyright>
// <author>Martin Tall</author>
// <email>info@martintall.com</email>

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using GazeTrackerClient;
using GazeTrackerUI.CalibrationUI;
using GazeTrackerUI.Tools;
using GazeTrackerUI.Network;
using GazeTrackerUI.SettingsUI;
using GazeTrackerUI.TrackerViewer;
using GazeTrackingLibrary;
using GazeTrackingLibrary.Logging;
using GazeTrackingLibrary.Utils;
using GTCommons;
using GTCommons.Enum;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Windows.Point;
using Settings = GTSettings.Settings;

namespace GazeTrackerUI
{
  // System classes

  // GazeTracker classes

  //using GazeTrackingLibrary.Illumination;

  public partial class GazeTrackerUIMainWindow
  {
    private readonly CrosshairDriver crosshairDriver = new CrosshairDriver();
    private readonly MouseDriver mouseDriver = new MouseDriver();
    private Process clientUIProc;
    private bool isRunning;
    private MessageWindow msgWindow;

    #region Constructor / Init methods

    public GazeTrackerUIMainWindow()
    {
      // Little fix for colorschema (must run before initializing)
      ComboBoxBackgroundColorFix.Initialize();

      // Register for special error messages
      ErrorLogger.TrackerError += tracker_OnTrackerError;

      this.ContentRendered += new EventHandler(GazeTrackerUIMainWindow_ContentRendered);

      InitializeComponent();

    }

    //Event Handler that runs when the Windows ContentRendered event occurs
    private void GazeTrackerUIMainWindow_ContentRendered(object sender, EventArgs e)
    {
      // Load GTSettings
      Settings.Instance.LoadLatestConfiguration();

      // Camera initialization and start frame grabbing
      if (GTHardware.Camera.Instance.DeviceType != GTHardware.Camera.DeviceTypeEnum.None)
      {
        // If DirectShow camera, init using saved settings
        if (GTHardware.Camera.Instance.DeviceType == GTHardware.Camera.DeviceTypeEnum.DirectShow)
          GTHardware.Camera.Instance.SetDirectShowCamera(GTSettings.Settings.Instance.Camera.DeviceNumber, GTSettings.Settings.Instance.Camera.DeviceMode);
        else
          GTHardware.Camera.Instance.Device.Initialize();

        GTHardware.Camera.Instance.Device.Start();
      }
      else
      {
        // No camera detected, display message and quit
        ShowMessageNoCamera();
        //this.Close();
        //return;
      }

      // Create Tracker
      //tracker = new Tracker(GTCommands.Instance); // Hook up commands and events to tracker

      SettingsWindow.Instance.Title = "SettingsWindow"; // Just touch it..


      // Video preview window (tracker draws visualization)
      videoImageControl.Start();

      // Events
      RegisterEventListners();

      // Show window
      Show();

      // Set this process to real-time priority
      Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
    }

    #endregion


    private void StartStop(object sender, RoutedEventArgs e)
    {
      if (Tracker.Instance.Calibration.IsCalibrated == false)
      {
        msgWindow = new MessageWindow("You need to calibrate before starting");
        msgWindow.Show();
        return;
      }

      // Starting
      if (!isRunning)
      {
        //Eye mouse region
        // Start eye mouse (register listner for gazedata events)
        if (Settings.Instance.Processing.EyeMouseEnabled)
        {
            if (Settings.Instance.Processing.EyeMouseSmooth)
                Tracker.Instance.GazeDataSmoothed.GazeDataChanged += mouseDriver.Move;
            else
                Tracker.Instance.GazeDataRaw.GazeDataChanged += mouseDriver.Move;
        }

        
        //Crosshair region
        if (Settings.Instance.Processing.EyeCrosshairEnabled)
        {
            crosshairDriver.Show();
            if (Settings.Instance.Processing.EyeMouseSmooth)
                Tracker.Instance.GazeDataSmoothed.GazeDataChanged += crosshairDriver.Move;
            else
                Tracker.Instance.GazeDataRaw.GazeDataChanged += crosshairDriver.Move;
        }

        // Start logging (if enabled)
        Tracker.Instance.LogData.IsEnabled = GTSettings.Settings.Instance.FileSettings.LoggingEnabled;

        //Start/Stop button used to change to stop here

        isRunning = true;
      }

      // Stopping
      else
      {
          //Eye mouse region
          // Stop eye mouse (unregister events)
          if (Settings.Instance.Processing.EyeMouseEnabled)
          {
              if (Settings.Instance.Processing.EyeMouseSmooth)
                  Tracker.Instance.GazeDataSmoothed.GazeDataChanged -= mouseDriver.Move;
              else
                  Tracker.Instance.GazeDataRaw.GazeDataChanged -= mouseDriver.Move;
          }

        //Crosshair region
        if (Settings.Instance.Processing.EyeCrosshairEnabled)
        {
            if (Settings.Instance.Processing.EyeMouseSmooth)
                Tracker.Instance.GazeDataSmoothed.GazeDataChanged -= crosshairDriver.Move;
            else
                Tracker.Instance.GazeDataRaw.GazeDataChanged -= crosshairDriver.Move;

            crosshairDriver.Hide();
        }

        if (Tracker.Instance.LogData.IsEnabled)
            Tracker.Instance.LogData.IsEnabled = false; // Will stop and close filestream

        //Start/Stop button used to change to start here

        isRunning = false;
      }
    }


    #region Events

    private void RegisterEventListners()
    {
        //Settings region
        GTCommands.Instance.Settings.OnSettingsButton += ShowSetupWindow; //Added to deal with loss of the 'Settings' button
        GTCommands.Instance.Settings.OnSettings += OnSettings;

        //Camera region
        GTCommands.Instance.Camera.OnCameraChange += OnCameraChanged;

        //TrackerViewer Region
        GTCommands.Instance.TrackerViewer.OnVideoDetach += OnVideoDetach;
        GTCommands.Instance.TrackerViewer.OnTrackBoxShow += OnTrackBoxShow;
        GTCommands.Instance.TrackerViewer.OnTrackBoxHide += OnTrackBoxHide;

        //Calibration region
        GTCommands.Instance.Calibration.OnCalibrationButton += Calibrate; //Added to deal with loss of the 'Calibrate' button
        GTCommands.Instance.Calibration.OnAccepted += OnCalibrationAccepted;
        GTCommands.Instance.Calibration.OnStart += OnCalibrationStart;
        GTCommands.Instance.Calibration.OnRunning += OnCalibrationRunning;

        GTCommands.Instance.Calibration.OnPointStart += OnPointStart;
        //GTCommands.Instance.Calibration.OnPointStart += new GTCommons.Events.CalibrationPointEventArgs.CalibrationPointEventHandler(OnPointStart);
        //GTCommands.Instance.Calibration.OnPointStart += new GazeTrackerUI.Calibration.Events.CalibrationPointEventArgs.CalibrationPointEventHandler(OnPointStart);

        GTCommands.Instance.Calibration.OnAbort += OnCalibrationAbort;
        GTCommands.Instance.Calibration.OnEnd += OnCalibrationEnd;

        //Misc region
        //Devin removes this because we dont want a network client
        //GTCommands.Instance.OnNetworkClient += OnNetworkClient;

        //"This window" region
        Activated += Window1_Activated;
        Deactivated += Window1_Deactivated;
        KeyDown += KeyDownAction;
    }

    private void ExpanderVisualization_Collapsed(object sender, RoutedEventArgs e)
    {
      videoImageControl.Overlay.GridVisualization.Visibility = Visibility.Collapsed;
    }

    private void ExpanderVisualization_Expanded(object sender, RoutedEventArgs e)
    {
      videoImageControl.Overlay.GridVisualization.Visibility = Visibility.Visible;
    }


    private void tracker_OnTrackerError(string message)
    {
      msgWindow = new MessageWindow { Text = message };
      msgWindow.Show();
    }

    private void KeyDownAction(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Escape:
          if (isRunning)
            StartStop(null, null);
          break;

        case Key.C:
          GTCommands.Instance.Calibration.Start();
          break;

        case Key.S:
          if (!isRunning)
            StartStop(null, null);
          break;
      }
    }

    #endregion


    #region On GTCommands -> actions

    #region Settings

    private void ShowSetupWindow(object sender, RoutedEventArgs e)
    {
      //if (SettingsWindow.Instance.Visibility != Visibility.Collapsed) return;
      SettingsWindow.Instance.Visibility = Visibility.Visible;
      SettingsWindow.Instance.Focus();

      Settings.Instance.Visualization.VideoMode = VideoModeEnum.Processed;

      if (SettingsWindow.Instance.HasBeenMoved != false) return;
      SettingsWindow.Instance.Left = Left + Width + 5;
      SettingsWindow.Instance.Top = Top;
    }

    private static void OnSettings(object sender, RoutedEventArgs e)
    {
      SettingsWindow.Instance.Visibility = Visibility.Visible;
    }

    #endregion

    #region TrackerViewer

    private void OnVideoDetach(object sender, RoutedEventArgs e)
    {
      int width = 0;
      int height = 0;

      VideoViewer.Instance.SetSizeAndLabels();

      // If ROI has been set display at twice the image size
      if (GTHardware.Camera.Instance.Device != null && GTHardware.Camera.Instance.Device.IsROISet)
      {
        width = GTHardware.Camera.Instance.ROI.Width * 2;
        height = GTHardware.Camera.Instance.ROI.Height * 2;
      }
      else
      {
        width = GTHardware.Camera.Instance.Width;
        height = GTHardware.Camera.Instance.Height;
      }

      int posX = Convert.ToInt32(Left - width - 5);
      int posY = Convert.ToInt32(Top);

      this.videoImageControl.VideoOverlayTopMost = false;

      VideoViewer.Instance.ShowWindow(width, height);
    }

    private void OnTrackBoxShow(object sender, RoutedEventArgs e)
    {
      //if (trackBoxUC.Visibility == Visibility.Collapsed)
      //    trackBoxUC.Visibility = Visibility.Visible;
    }

    private void OnTrackBoxHide(object sender, RoutedEventArgs e)
    {
      //if (trackBoxUC.Visibility == Visibility.Visible)
      //    trackBoxUC.Visibility = Visibility.Collapsed;
    }

    private void OnROIChange(Rectangle newROI)
    {
      //Dispatcher.Invoke
      //    (
      //        DispatcherPriority.ApplicationIdle,
      //        new Action
      //            (
      //            delegate { trackBoxUC.UpdateROI(newROI); }
      //            )
      //    );
    }

    #endregion

    #region Calibration

    private void Calibrate(object sender, RoutedEventArgs e)
    {
      GTCommands.Instance.Calibration.Start();
      videoImageControl.VideoOverlayTopMost = false;
    }

    private void OnCalibrationStart(object sender, RoutedEventArgs e)
    {
      CalibrationWindow.Instance.Reset();
      CalibrationWindow.Instance.Show();
      CalibrationWindow.Instance.Start();
    }

    private void OnCalibrationRunning(object sender, RoutedEventArgs e)
    {
      Tracker.Instance.CalibrationStart();
    }

    private void OnCalibrationAccepted(object sender, RoutedEventArgs e)
    {
      CalibrationWindow.Instance.Close();
      WindowState = WindowState.Normal;
      this.videoImageControl.VideoOverlayTopMost = true;

      Tracker.Instance.CalibrationAccepted();
    }

    private void OnPointStart(object sender, RoutedEventArgs e)
    {
      var control = sender as CalibrationControl;
      if (control != null) Tracker.Instance.CalibrationPointStart(control.CurrentPoint.Number, control.CurrentPoint.Point);
      e.Handled = true;
    }

    //private void OnPointStart(object sender, GazeTrackerUI.Calibration.Events.CalibrationPointEventArgs e)
    //{
    //    tracker.CalibrationPointStart(e.Number, e.Point);
    //    e.Handled = true;
    //}

    //private void OnPointEnd(object sender, CalibrationPointEventArgs e)
    //{
    //    tracker.CalibrationPointEnd(e.Number, e.Point);
    //    e.Handled = true;
    //}

    private void OnCalibrationAbort(object sender, RoutedEventArgs e)
    {
      CalibrationWindow.Instance.Stop();
      Tracker.Instance.CalibrationAbort();
    }

    private void OnCalibrationEnd(object sender, RoutedEventArgs e)
    {
      Tracker.Instance.CalibrationEnd();
    }


    #endregion

    #endregion


    #region Camera/Video viewing

    private void OnCameraChanged(object sender, RoutedEventArgs e)
    {
      Tracker.Instance.SetCamera(Settings.Instance.Camera.DeviceNumber, Settings.Instance.Camera.DeviceMode);

      Point oldWinPos = new Point(VideoViewer.Instance.Top, VideoViewer.Instance.Left);
      VideoViewer.Instance.Width = Tracker.Instance.VideoWidth + videoImageControl.Margin.Left + videoImageControl.Margin.Right;
      VideoViewer.Instance.Height = Tracker.Instance.VideoHeight + videoImageControl.Margin.Top + videoImageControl.Margin.Bottom;
    }


    private void ShowMessageNoCamera()
    {
      msgWindow = new MessageWindow();
      msgWindow.Text = "The GazeTracker was unable to connect a camera. \n" +
                       "Make sure that the device is connected and that the device drivers are installed. " +
                       "Verified configurations can be found in our forum located at http://forum.gazegroup.org";
      msgWindow.Show();
      ErrorLogger.WriteLine("Fatal error on startup, could not connect to a camera.");
      msgWindow.Closed += new EventHandler(msgWindowNoCamera_Closed);
    }

    private void msgWindowNoCamera_Closed(object sender, EventArgs e)
    {
      //Environment.Exit(Environment.ExitCode);
    }

    #endregion


    #region NetworkClient

    private void OnNetworkClient(object sender, RoutedEventArgs e)
    {
      NetworkClientLaunch();
    }

    private void NetworkClientLaunch()
    {
      if (Settings.Instance.Network.ClientUIPath == "" ||
          !File.Exists(Settings.Instance.Network.ClientUIPath))
      {
        var ofd = new OpenFileDialog();
        ofd.Title = "Select the GazeTrackerClientUI executable file";
        ofd.Multiselect = false;
        ofd.Filter = "Executables (*.exe)|*.exe*;*.xml|All Files|*.*";

        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
          string[] filePath = ofd.FileNames;
          Settings.Instance.Network.ClientUIPath = filePath[0];
        }
        else
        {
        }
      }

      if (!File.Exists(Settings.Instance.Network.ClientUIPath)) return;
      var psi = new ProcessStartInfo(Settings.Instance.Network.ClientUIPath);
      clientUIProc = new Process { StartInfo = psi };
      clientUIProc.Start();
    }

    #endregion


    #region Minimize/Activate/Close main app window

    private void AppMinimize(object sender, MouseButtonEventArgs e)
    {
      // If video is detached (seperate window), stop updating images and close the window)
      if (VideoViewer.Instance.WindowState.Equals(WindowState.Normal))
      {
        VideoViewer.Instance.videoImageControl.Stop(true);
        VideoViewer.Instance.WindowState = WindowState.Minimized;
      }

      // Stop updating images in small preview box
      this.videoImageControl.Stop(true);

      // Minimize settings window
      SettingsWindow.Instance.WindowState = WindowState.Minimized;

      // Mimimize the application window
      WindowState = WindowState.Minimized;
    }

    private void AppClose(object sender, MouseButtonEventArgs e)
    {
      // Save settings 
      SettingsWindow.Instance.SaveSettings();
      //CameraSettingsWindow.Instance.Close(); //is already closed - will force the class to reinitialize only to be closed again

      // Kill the ClientUI process (if initiated)
      if (clientUIProc != null && clientUIProc.HasExited == false)
        clientUIProc.Kill();

      // Cleaup tracker & release camera
      Tracker.Instance.Cleanup();

      // Close all windows (including Visibility.Collapsed & Hidden)
      for (int i = 0; i < Application.Current.Windows.Count; i++)
        Application.Current.Windows[i].Close();

      // Null tracker..
      //Tracker.Instance = null;

      // Force exit, now dammit!
      Environment.Exit(Environment.ExitCode);
    }

    private void Window1_Activated(object sender, EventArgs e)
    {
      if (Visibility == Visibility.Visible && Settings.Instance.Visualization.IsDrawing == false)
        this.videoImageControl.Start();

      SettingsWindow.Instance.WindowState = WindowState.Normal;
      SettingsWindow.Instance.Focus();
      this.Focus();


      videoImageControl.VideoOverlayTopMost = true;
    }

    private void Window1_Deactivated(object sender, EventArgs e)
    {
      if (WindowState.Equals(WindowState.Minimized))
        videoImageControl.Stop(true);

      videoImageControl.VideoOverlayTopMost = false;
    }

    #endregion


    #region DragWindow

    private void DragWindow(object sender, MouseButtonEventArgs args)
    {
      DragMove();
    }

    #endregion
  }
}