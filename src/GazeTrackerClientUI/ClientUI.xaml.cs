using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GazeTrackerClient;

namespace GazeTrackerClientUI
{
    public partial class ClientUI : Window
    {
        private readonly Client client;
        private readonly List<string> commandHistory;
        private readonly List<string> receivedData;

        private Thread clientReceiveThread;

        private int commandLogMaxLines = 13;
        private string InstanceCommand;
        private int receivedDataMaxLines = 25;


        public ClientUI()
        {
            ComboBoxBackgroundColorFix.Initialize();
            commandHistory = new List<string>(commandLogMaxLines + 1);
            receivedData = new List<string>(receivedDataMaxLines + 1);

            client = new Client();
            client.IPAddress = IPAddress.Parse("127.0.0.1");
            client.PortReceive = 6666;
            client.PortSend = 5555;
            client.ClientConnectionChanged += OnClientConnectionChanged;
            client.Calibration.OnEnd += Calibration_OnEnd;


            InitializeComponent();

            // Will use settings found in "GazeTrackerSettings.xml" or default values
            TextBoxIPAddress.Text = client.IPAddress.ToString();
            TextBoxPortOut.Text = client.PortSend.ToString();
            TextBoxPortIn.Text = client.PortReceive.ToString();

            // Just so we can do it in run-time (instead of applying it on Connect/Disconnect)
            CheckBoxListenToEvents.Checked += CheckBoxListenToEvents_Checked;
            CheckBoxListenToEvents.Unchecked += CheckBoxListenToEvents_Unchecked;

            SetCommandList();
        }

        #region Connect/OnConnect/ListnerThread

        private void ConnectToTracker(object sender, RoutedEventArgs e)
        {
            if (!client.IsRunning)
            {
                SaveConfig(null, null);

                eventNotifier.Client = client;
                eventNotifier.IsEnabled = CheckBoxListenToEvents.IsChecked.Value;

                string command = "Connecting...";
                UpdateCommandLog(command);

                clientReceiveThread = new Thread(StartClientReceiveThread);
                clientReceiveThread.Start();
            }
            else
            {
                try
                {
                    TextBlockData.Text = "";
                    clientReceiveThread.Abort();
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void OnClientConnectionChanged(object sender, bool isConnected)
        {
            if (isConnected)
            {
                UpdateCommandLog("Connected.");
                Dispatcher.Invoke((Action) delegate
                                               {
                                                   BtnConnect.Label = "Disconnect";
                                                   ComboBoxCommands.Visibility = Visibility.Visible;
                                               }, null);
            }
            else
            {
                UpdateCommandLog("Disconnected.");
                Dispatcher.Invoke((Action) delegate
                                               {
                                                   BtnConnect.Label = "Connect";
                                                   ComboBoxCommands.Visibility = Visibility.Collapsed;
                                               }, null);
            }
        }

        private void StartClientReceiveThread()
        {
            //client.GazeData.OnGazeData += new GazeDat   a.GazeDataHandler(GazeData_OnGazeData);
            client.GazeData.OnSmoothedGazeData += GazeData_OnGazeData;
            client.Connect();
        }

        #endregion

        #region Log display

        private long prevTime;

        private void GazeData_OnGazeData(GazeData gData)
        {
            Dispatcher.Invoke(
                (Action) delegate
                             {
                                 if (gData.TimeStamp == prevTime) // if its the same timestamp, don't bother..
                                     return;

								 string s = gData.TimeStamp + "\t" + gData.GazePositionX + "\t" +
											gData.GazePositionY + "\t" + gData.PupilDiameterLeft;

                                 // Only keep 15 log lines...
                                 if (receivedData.Count <= receivedDataMaxLines)
                                     receivedData.Add(s);
                                 else
                                 {
                                     for (int i = 0; i < receivedDataMaxLines; i++)
                                     {
                                         receivedData[i] = receivedData[i + 1];
                                     }

                                     receivedData[receivedDataMaxLines] = s;
                                 }

                                 // Clear textbox
                                 TextBlockData.Text = "";

                                 // List received data 
                                 foreach (string dataStr in receivedData)
                                 {
                                     TextBlockData.Text = TextBlockData.Text + dataStr + "\n";
                                 }

                                 prevTime = gData.TimeStamp;
                             }, DispatcherPriority.Background, null);
        }

        private void UpdateCommandLog(string command)
        {
            Dispatcher.Invoke(
                (Action) delegate
                             {
                                 if (commandHistory.Count <= commandLogMaxLines)
                                     commandHistory.Add(command);
                                 else
                                 {
                                     for (int i = 0; i < commandLogMaxLines; i++)
                                         commandHistory[i] = commandHistory[i + 1];

                                     commandHistory[commandLogMaxLines] = command;
                                 }

                                 TextBlockCommandLog.Text = "";

                                 foreach (string str in commandHistory)
                                 {
                                     TextBlockCommandLog.Text = TextBlockCommandLog.Text + str + "\n";
                                 }
                             }, null);
        }

        #endregion

        #region CommandList / Issue commands

        private void SetCommandList()
        {
            ComboBoxCommands.Items.Add(" ");

            ComboBoxCommands.Items.Add("Tracker Status - " + Commands.TrackerStatus);
            ComboBoxCommands.Items.Add("Tracker Parameters - " + Commands.EyeTrackerParameters);

            ComboBoxCommands.Items.Add("U.I Minimize - " + Commands.UIMinimize);
            ComboBoxCommands.Items.Add("U.I Restore - " + Commands.UIRestore);
            ComboBoxCommands.Items.Add("U.I Settingspanel - " + Commands.UISettings);

            ComboBoxCommands.Items.Add("Streaming Start - " + Commands.StreamStart);
            ComboBoxCommands.Items.Add("Streaming Stop - " + Commands.StreamStop);
            ComboBoxCommands.Items.Add("Streaming Format - " + Commands.StreamFormat);

            ComboBoxCommands.Items.Add("Calibration Start - " + Commands.CalibrationStart);
            ComboBoxCommands.Items.Add("Calibration Abort - " + Commands.CalibrationAbort);
            ComboBoxCommands.Items.Add("Calibration Validate - " + Commands.CalibrationValidate);
            ComboBoxCommands.Items.Add("Calibration Parameters - " + Commands.CalibrationParameters);
            ComboBoxCommands.Items.Add("Calibration Quality - " + Commands.CalibrationQuality);

            ComboBoxCommands.Items.Add("Log Start - " + Commands.LogStart);
            ComboBoxCommands.Items.Add("Log Stop - " + Commands.LogStop);
            ComboBoxCommands.Items.Add("Log WriteLine - " + Commands.LogWriteLine);
            ComboBoxCommands.Items.Add("Log Path Set - " + Commands.LogPathSet);
            ComboBoxCommands.Items.Add("Log Path Get - " + Commands.LogPathGet);

            ComboBoxCommands.Items.Add("Camera Device&Mode - " + Commands.CameraSetDeviceMode);
            ComboBoxCommands.Items.Add("Camera Parameters - " + Commands.CameraParameters);

            ComboBoxCommands.Items.Add("MouseControl_Enable - ");
            ComboBoxCommands.Items.Add("MouseControl_Disable - ");

            ComboBoxCommands.SelectionChanged += ComboBoxCommands_SelectionChanged;
        }


        private void ComboBoxCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxCommands.SelectedValue.ToString() == " ")
                return;

            // Extract command from string, ex. "Log Start - LOG_START"
            char[] splitter = {'-'};
            string[] cmd = ComboBoxCommands.SelectedValue.ToString().Split(splitter);
            InstanceCommand = cmd[1].Substring(1, cmd[1].Length - 1);

            // Show form for parameters is command has any, will call SendCommand on submit
            if (Commands.CommandHasParameter(InstanceCommand))
                GridParameters.Visibility = Visibility.Visible;
            else
                SendCommand(InstanceCommand);
        }


        private void SendCommand(string command)
        {
            UpdateCommandLog(InstanceCommand);

            char[] seperator = {' '};
            string[] cmds = command.Split(seperator, 2);
            string cmdStr = cmds[0];

            string paramStr = "";

            if (cmds.Length > 1)
                paramStr = cmds[1];

            switch (cmdStr)
            {
                    // Calibration

                case Commands.CalibrationStart:
                    client.Calibration.Start();
                    WindowState = WindowState.Minimized;
                    break;
                case Commands.CalibrationAbort:
                    client.Calibration.Abort();
                    break;

                case Commands.CalibrationParameters:
                    var calParams = new CalibrationParameters();
                    calParams.ExtractParametersFromString(paramStr);
                    client.Calibration.CalibrationParameters(calParams);
                    break;

                    // Stream

                case Commands.StreamStart:
                    client.Stream.StreamStart();
                    break;
                case Commands.StreamStop:
                    client.Stream.StreamStop();
                    break;

                    // Log

                case Commands.LogStart:
                    client.Log.Start();
                    break;

                case Commands.LogStop:
                    client.Log.Stop();
                    break;

                case Commands.LogPathSet:
                    client.Log.SetLogPath(paramStr);
                    break;
                case Commands.LogPathGet:
                    client.Log.GetLogPath();
                    break;

                case Commands.LogWriteLine:
                    client.Log.WriteLine(paramStr);
                    break;

                    // Camera 

                case Commands.CameraParameters:
                    client.Camera.ApplyParameters(paramStr);
                        // available as override w/ string, eg. Brightness=100,Contrast=255 etc.
                    break;


                    // U.I

                case Commands.UIMinimize:
                    client.UIControl.Minimize();
                    break;

                case Commands.UIRestore:
                    client.UIControl.Restore();
                    break;

                case Commands.UISettings:
                    client.UIControl.Settings();
                    break;

                case Commands.UISettingsCamera:
                    client.UIControl.SettingsCamera();
                    break;

                    // Mouse control

                case "MouseControl_Enable:":
                    client.MouseControl.IsEnabled = true;
                    break;
                case "MouseControl_Disable:":
                    client.MouseControl.IsEnabled = false;
                    break;
            }
        }


        private void SetParameters(object sender, RoutedEventArgs e)
        {
            GridParameters.Visibility = Visibility.Collapsed;

            SendCommand(InstanceCommand + " " + TextBoxParameters.Text);

            //client.SendCommand(InstanceCommand, TextBoxParameters.Text);
        }


        private void GridParametersClose(object sender, MouseButtonEventArgs e)
        {
            GridParameters.Visibility = Visibility.Collapsed;
            ComboBoxCommands.SelectedIndex = 0;
        }

        #endregion

        #region Configure settings

        private void ShowConfig(object sender, RoutedEventArgs e)
        {
            // Toggle visibility of config menu
            if (GridConfig.Visibility == Visibility.Collapsed)
                GridConfig.Visibility = Visibility.Visible;
            else
                GridConfig.Visibility = Visibility.Collapsed;
        }

        private void CheckBoxListenToEvents_Unchecked(object sender, RoutedEventArgs e)
        {
            eventNotifier.IsEnabled = false;
        }

        private void CheckBoxListenToEvents_Checked(object sender, RoutedEventArgs e)
        {
            eventNotifier.IsEnabled = true;
        }


        private void SaveConfig(object sender, RoutedEventArgs e)
        {
            bool error = false;
            try
            {
                int portIn = Convert.ToInt32(TextBoxPortIn.Text);

                if (portIn >= 1 && portIn <= 65535)
                    client.PortReceive = portIn;
                else
                    throw new ArgumentException("Invalid UDP port number");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please enter a valid port number between 1-65535.");
                TextBoxPortIn.Focus();
                error = true;
            }

            try
            {
                int portOut = Convert.ToInt32(TextBoxPortOut.Text);

                if (portOut >= 1 && portOut <= 65535)
                    client.PortSend = portOut;
                else
                    throw new ArgumentException("Invalid TCP/IP port number");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please enter a valid port number between 1-65535.");
                TextBoxPortOut.Focus();
                error = true;
            }

            try
            {
                IPAddress ipAdd = IPAddress.Parse(TextBoxIPAddress.Text);
                client.IPAddress = ipAdd;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please enter a valid IP address such as 127.0.0.1");
                TextBoxIPAddress.Focus();
                error = true;
            }

            if (!error)
            {
                client.Settings.WriteConfigFile();
                GridConfig.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region WindowManagment / onCalibrationEnd / minimize / close etc.

        private void Calibration_OnEnd(int quality)
        {
            WindowState = WindowState.Normal;
        }

        private void AppMinimize(object sender, MouseButtonEventArgs e)
        {
            // Mimimize the application window
            WindowState = WindowState.Minimized;
        }

        private void AppClose(object sender, MouseButtonEventArgs e)
        {
            client.Disconnect();

            if (clientReceiveThread != null)
                clientReceiveThread.Abort();

            Environment.Exit(Environment.ExitCode);
        }


        private void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        #endregion
    }
}