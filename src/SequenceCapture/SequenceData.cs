using System;
using System.Collections.Generic;
using System.Xml;
using Emgu.CV.Structure;
using System.ComponentModel;
using System.IO;

namespace SequenceCapture
{
    public class SequenceData
    {
        
        #region Variables

        private string baseFolder = "Sequences";
        private long id = 0;
        List<ImageData> imageDb = new List<ImageData>();

        // Info
        private string username;
        private string deviceName;
        private int irSourceCount = 0;
        private string lens;
        private string lensMM;
        private string lensIris;
        private string notes;
        private CameraViewEnum cameraView;

        public enum CameraViewEnum
        {
            Binocular = 0,
            Monocular = 1,
            Headmounted = 2,
        }


        #endregion

        #region Constructor

        public SequenceData()
        {

        }

        #endregion

        #region Get/Set

        public List<ImageData> ImageDb
        {
            get { return imageDb; }
        }

        public long ID
        {
            get { return id; }
        }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public string DeviceName
        {
            get { return deviceName; }
            set { deviceName = value; }
        }

        public int IRSourceCount
        {
            get { return irSourceCount; }
            set { irSourceCount = value; }
        }

        public string Lens
        {
            get { return lens; }
            set { lens = value; }
        }

        public string LensMM
        {
            get { return lensMM; }
            set { lensMM = value; }
        }

        public string LensIris
        {
            get { return lensIris; }
            set { lensIris = value; }
        }

        public CameraViewEnum CameraView
        {
            get { return cameraView; }
            set { cameraView = value; }
        }

        public string Notes
        {
            get { return notes; }
            set { notes = value; }
        }

        public string GetFolder()
        {
            return baseFolder + "\\" + id.ToString();
        }

        #endregion

        #region Public methods

        public void AddImage(Emgu.CV.Image<Gray, byte> image, System.Windows.Point position)
        {
            if(id == 0)
            {
                id = GenerateUniqueID();

                // Create directory
                if (Directory.Exists(baseFolder + "\\" + id.ToString()) == false)
                    Directory.CreateDirectory(baseFolder + "\\" + id.ToString());
            }

            // add image to memory storage (saved on completed sequence)
            imageDb.Add(new ImageData(image, position));
        }

        public void Save(BackgroundWorker worker)
        {
            // Passing worker from UI thread, it listens for progress change events
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync(baseFolder + "\\" + id.ToString() + "\\");
        }

        public void LoadLatest()
        {
            int numSessions = 0;

            try
            {
                string[] directories = Directory.GetDirectories(baseFolder);
                string lastDir;

                // Append entry with timestamp (creation)
                for (int j = 0; j < directories.Length; j++)
                {
                    string lastWriteTime = File.GetLastWriteTime(directories[j]).ToString("yyyyMMddHHmmss");
                    directories[j] = lastWriteTime + directories[j];
                    numSessions++;
                }

                // Sort the dir-list on time
                Array.Sort(directories);

                if (numSessions > 0)
                {
                    // Newest directory (last in list)
                    string newestDir = Path.GetFileName(directories[directories.Length - 1].Substring(15));
                    // Get xml files, should only be one in the directory
                    string[] xmlFiles = Directory.GetFiles(baseFolder + "\\" + newestDir, "*.xml");

                    if (xmlFiles.Length != 0 && File.Exists(xmlFiles[0]))
                    {
                        // Parse XML and set variables                        
                        LoadInfo(xmlFiles[0]);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine("SequenceData.cs, rrror loading latest XML file. Message: " + ex.Message);
            }
        }

        #endregion

        #region Private methods

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            SaveInfo();

            string path = e.Argument.ToString();
            int counter = 0;

            foreach (ImageData img in imageDb)
            {
                if(img.image == null || img.image.Width == 0 || img.image.Data.Length < 1)
                    continue;

                counter++;
                string imgName = counter.ToString();

                imgName += "_" + img.position.X + "_" + img.position.Y + ".jpg";
                img.image.Save(path + imgName);
                img.image.Dispose();

                worker.ReportProgress(Convert.ToInt32(counter*100/imageDb.Count));
            }
        }

        private void SaveInfo()
        {
            FileStream fs = new FileStream(baseFolder + "\\" + id.ToString() + "\\" + id.ToString() + ".xml", FileMode.Create);
            XmlWriter w = XmlWriter.Create(fs);

            w.WriteStartDocument();
            w.WriteStartElement("SequenceData");

            // Write a product.
            w.WriteStartElement("Info");
            w.WriteAttributeString("ID", id.ToString());
            w.WriteElementString("UserName", username);
            w.WriteElementString("IRSources", IRSourceCount.ToString());
            w.WriteElementString("Device", deviceName);
            w.WriteElementString("Width", GTHardware.Camera.Instance.Width.ToString());
            w.WriteElementString("Height", GTHardware.Camera.Instance.Height.ToString());
            w.WriteElementString("ImageCount", imageDb.Count.ToString());
            w.WriteElementString("Lens", lens);
            w.WriteElementString("LensMM", lensMM);
            w.WriteElementString("LensIris", lensIris);
            w.WriteElementString("CameraView", Enum.GetName(typeof (CameraViewEnum), cameraView));
            w.WriteElementString("Notes", notes);

            w.WriteEndElement();

            w.WriteEndDocument();
            w.Flush();
            fs.Close();
        }

        private void LoadInfo(string xmlFilePath)
        {
            if (File.Exists(xmlFilePath))
            {
                XmlReader xmlReader = new XmlTextReader(xmlFilePath);
                string sName = "";

                if (xmlReader != null)
                {
                    try
                    {
                        while (xmlReader.Read())
                        {
                            switch (xmlReader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    sName = xmlReader.Name;
                                    break;
                                case XmlNodeType.Text:
                                    switch (sName)
                                    {
                                        case "UserName":
                                            Username = xmlReader.Value;
                                            break;
                                        case "IRSources":
                                            IRSourceCount = Convert.ToInt32(xmlReader.Value);
                                            break;
                                        case "Lens":
                                            Lens = xmlReader.Value;
                                            break;
                                        case "LensMM":
                                            LensMM = xmlReader.Value;
                                            break;
                                        case "LensIris":
                                            LensIris = xmlReader.Value;
                                            break;
                                        case "CameraView":
                                            CameraView =
                                                (CameraViewEnum)
                                                Enum.Parse(typeof(CameraViewEnum), xmlReader.Value, true);
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private static long GenerateUniqueID()
        {
            Random rand = new Random();
            DateTime dt = DateTime.Now;

            // Eg. 20100115122354 + random value 1000 to 9999 
            // eg. maximum 10,000 unique sessions per every second of every minute, every hour, day, month
            // ought be unqiue enough..

            string today = String.Format("{0:yyyyMMddHHmmss}", dt);
            string strID = today + rand.Next(1000, 9999).ToString();
            long id = long.Parse(strID);

            //CheckIfIDExists(id); not sure we need it

            return id;
        }

        #endregion

    }


    public class ImageData
    {
        public Emgu.CV.Image<Gray, byte> image;
        public System.Windows.Point position;

        public ImageData(Emgu.CV.Image<Gray, byte> image, System.Windows.Point position)
        {
            this.image = image;
            this.position = position;
        }
    }

}
