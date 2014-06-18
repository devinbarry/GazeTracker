using System.Windows;
using System.Windows.Controls;
using GazeTrackerClient;

namespace GazeTrackerClientUI
{
    /// <summary>
    /// Interaction logic for EventNotifierControl.xaml
    /// </summary>
    public partial class EventNotifierControl : UserControl
    {
        private Client client;

        private bool isEnabled;


        public EventNotifierControl()
        {
            InitializeComponent();
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;

                if (isEnabled)
                    RegisterForEvents();
                else
                    UnRegisterForEvents();
            }
        }

        public Client Client
        {
            set { client = value; }
        }

        public void Show(string command)
        {
            Show(command, " ");
        }

        public void Show(string command, string param)
        {
            if (isEnabled)
            {
                MessageBox.Show("Event: " + command + "\n Parameters: " + param);

                // ToDo: Implement thread safe....

                //this.LabelCmd.Content = command;
                //this.LabelParameters.Content = param;
                //this.Visibility = Visibility.Visible;
            }
        }


        private void RegisterForEvents()
        {
            if (client == null)
                return;

            client.Calibration.OnStart += Calibration_OnStart;
            client.Calibration.OnEnd += Calibration_OnEnd;
            client.Calibration.OnAbort += Calibration_OnAbort;
            client.Calibration.OnParameters += Calibration_OnParameters;

            client.Log.OnStart += Log_OnStart;
            client.Log.OnStop += Log_OnStop;
            client.Log.OnPathChange += Log_OnPathChange;
        }

        private void UnRegisterForEvents()
        {
            if (client == null)
                return;

            client.Calibration.OnStart -= Calibration_OnStart;
            client.Calibration.OnEnd -= Calibration_OnEnd;
            client.Calibration.OnAbort -= Calibration_OnAbort;
            client.Calibration.OnParameters -= Calibration_OnParameters;

            client.Log.OnStart -= Log_OnStart;
            client.Log.OnStop -= Log_OnStop;
            client.Log.OnPathChange -= Log_OnPathChange;
        }


        private void Calibration_OnStart(int numberOfPoints)
        {
            Show("Calibration started, number of points:", numberOfPoints.ToString());
        }

        private void Calibration_OnAbort(bool success)
        {
            Show("Calibration aborted");
        }

        private void Calibration_OnEnd(int quality)
        {
            Show("Calibration end, quality: ", quality.ToString());
        }

        private void Calibration_OnParameters(CalibrationParameters calParams)
        {
            Show("Calibration parameters changed.", calParams.ParametersAsString);
        }


        private void Log_OnStart()
        {
            Show("Log started");
        }

        private void Log_OnPathChange(string newPath)
        {
            Show("Log path changed", newPath);
        }

        private void Log_OnStop()
        {
            Show("Log stopped");
        }


        private void CloseIt(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}