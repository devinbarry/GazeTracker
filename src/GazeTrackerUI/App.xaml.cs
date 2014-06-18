using System;
using System.IO;
using System.IO.Pipes;
using System.Timers;
using System.Windows;
using System.Text;
using System.Linq;
using System.Threading;
using log4net;
using GazeTrackerUI;
using GTCommons;

[assembly: log4net.Config.XmlConfigurator(ConfigFileExtension = "log4net", Watch = true)]

namespace GazeTrackerUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;
        // private static System.Timers.Timer singleTimer;

        void App_Startup(object sender, StartupEventArgs e)
        {
            // Application is running
            System.Console.WriteLine("App_Startup method has been called...");
            
            //Console.WriteLine("Starting timers");
            //singleTimer = new System.Timers.Timer();
            //singleTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            // Set the Interval to 15 seconds.
            //singleTimer.Interval = 15000;
            //singleTimer.Enabled = true;

            Thread pipesServerThread = new Thread(new ThreadStart(pipesServer));
            pipesServerThread.Start();

        }
        
        // Specify what you want to happen when the Elapsed event is raised.
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer has elapsed!");
            // singleTimer.Enabled = false; //disable timer
            calibrate(true);
            //settings(false);
        }

        //Start the eye mouse
        private static void startStop(Boolean fake)
        {
            if (fake)
            {
                Console.WriteLine("NOT Start mouse!");
            }
            else
            {
                Console.WriteLine("Starting mosue!");
                //Devin doesn't know what to do here yet
            }
        }

        //raise the CalibrateButton event
        private static void calibrate(Boolean fake)
        {
            if (fake)
            {
                Console.WriteLine("NOT Starting Calibrate!!");
            }
            else
            {
                Console.WriteLine("Starting Calibrate!!");
                //Raise CalibrationButton event
                GTCommands.Instance.Calibration.CalibrationButton();
            }
        }

        //raise the SettingsButton event
        private static void settings(Boolean fake)
        {
            if (fake)
            {
                Console.WriteLine("NOT Starting Settings!!");
            }
            else
            {
                Console.WriteLine("Starting Settings!!");
                //Raise SettingsButton event
                GTCommands.Instance.Settings.SettingsButton();
            }
        }

        private static void pipesServer()
        {
            while (true)
            {
                Thread pipesThread = new Thread(new ThreadStart(namedPipesApp));
                pipesThread.Start();
                pipesThread.Join();
            }
        }

        private static void namedPipesApp()
        {
            // Open the named pipe.
            var server = new NamedPipeServerStream("NPtest");

            Console.WriteLine("Waiting for connection...");
            server.WaitForConnection();

            var br = new BinaryReader(server);
            var bw = new BinaryWriter(server);

            while (true)
            {
                try
                {
                    var len = (int)br.ReadUInt32(); // Read string length
                    //Console.WriteLine("Read length of \"{0}\"", len);
                    var str = new string(br.ReadChars(len)); // Read string
                    //Console.WriteLine("Read: \"{0}\"", str);

                    Console.WriteLine("Connected and received data: {0}", str);
                    if (str == "settings")
                    {
                        settings(false);
                    }
                    else if (str == "calibrate")
                    {
                        calibrate(false);
                    }
                    else if (str == "start")
                    {
                        startStop(true);
                    }
                    else
                    {
                        Console.WriteLine("Unknown value \"{0}\"", str);
                    }
                }
                catch (EndOfStreamException)
                {
                    break; // When client disconnects
                }
            }
            Console.WriteLine("Client disconnected.");
            server.Close();
            server.Dispose();
        }

    }
}
