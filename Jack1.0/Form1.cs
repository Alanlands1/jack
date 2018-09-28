using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Runtime.InteropServices;
using System.Diagnostics;
using AIMLbot;
using System.IO.Ports;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.IO;
using System.Net;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Timers;
using Emgu.CV.CvEnum;
using System.Runtime.CompilerServices;

namespace Jack1._0
{
    public partial class Form1 : Form
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(int Msg, System.Windows.Forms.Keys wParam, int lParam);

        private string conn;
        private MySqlConnection connect;

        SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        static ChatBot bot;
        public Boolean state = false;
        Choices list = new Choices();

       
        Random rnd = new Random();
        Boolean o = true;
        public string f ="0";


        SerialPort ardo = new SerialPort();
        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
        SpeechRecognitionEngine sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));



        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;
        private Capture videoCapture;
        private HaarCascade haarCascade;
        public Image<Bgr, Byte> bgrFrame = null;
        public Image<Gray, Byte> detectedFace = null;
        private List<FaceData> faceList = new List<FaceData>();
        public List<Image<Gray, Byte>> imageList = new List<Image<Gray, byte>>();
        public List<string> lList = new List<string>();
        #region FaceName
        private string faceName;
        public string FaceName
        {
            get { return faceName; }
            set
            {
                faceName = value.ToUpper();
                //say("Hello " + faceName);
               // lblFaceName.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { lblFaceName.Content = faceName; }));
                NotifyPropertyChanged();
            }
        }
        #endregion
        

        #region Constructor
      
        #endregion

        #region Event
        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        
        
       
        
        
        public void GetFacesList()
        {

            //haar cascade classifier
            haarCascade = new HaarCascade(Config.HaarCascadePath);
            faceList.Clear();
            string line;
            FaceData faceInstance = null;
            //split face text file
            StreamReader reader = new StreamReader(Config.FaceListTextFile);
            while ((line = reader.ReadLine()) != null)
            {
                string[] lineParts = line.Split(':');
                faceInstance = new FaceData();
                faceInstance.FaceImage = new Image<Gray, byte>(Config.FacePhotosPath + lineParts[0] + Config.ImageFileExtension);
                faceInstance.PersonName = lineParts[1];
                faceList.Add(faceInstance);

            }
            foreach (var face in faceList)
            {
                imageList.Add(face.FaceImage);
                lList.Add(face.PersonName);
            }
            reader.Close();
        }

        
        public void FrameProduced(object sender, EventArgs e)
        {
              try
                {
                //GetFacesList();
                //for emgu cv bug
                imgCamera.Visible = true;
                bgrFrame = videoCapture.QueryFrame();
                Image<Gray, byte> grayframe = bgrFrame.Convert<Gray, byte>();
                    //detect face
                    MCvAvgComp[][] faces = grayframe.DetectHaarCascade(haarCascade, 1.2, 10, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(20, 20));
                    
                    FaceName = "No face detected";
                    foreach (var face in faces[0])
                    {
                    
                        bgrFrame.Draw(face.rect, new Bgr(255, 255, 0), 2);
                        detectedFace = bgrFrame.Copy(face.rect).Convert<Gray, byte>();
                        if (imageList.ToArray().Length != 0)
                        {
                            MCvTermCriteria termCrit = new MCvTermCriteria(lList.Count, 0.001);
                            //Eigen Face Algorithm
                            EigenObjectRecognizer recognizer = new EigenObjectRecognizer(imageList.ToArray(), lList.ToArray(), 1000 , ref termCrit);
                            string faceName = recognizer.Recognize(detectedFace.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC));
                            if (faceName == "Alan Lands")
                            {
                                say(" Welcome Sir");
                                databind(faceName);
                                f = "1";
                            }
                            else
                            {
                             textBox2.Visible = true;
                             button2.Visible = true;
                            //say("Hello");

                        }
                      
                        
                        
                        
                        }
                        else
                        {
                            FaceName = "Please Add Face";
                        // say("Pls Add Face");
                        if (detectedFace == null)
                        {
                            MessageBox.Show("No face detected.");
                            return;
                        }
                        
                        //Save detected face
                        textBox2.Visible = true;
                        button2.Visible = true;
                       
                        //say("Sorry I don't know who are you?");
                        label123.Text = faceName;
                    }

                }
                if (f != "1")
                {
                    imgCamera.Image = bgrFrame;
                }
                else
                {
                    
                    videoCapture.Dispose();
                    /*Form1 obj = new Form1();
                    obj.Show();*/
                    imgCamera.Visible = false;
                    Application.Idle -= new EventHandler(FrameProduced);
                    // sre.SpeechRecognized += sre_SpeechRecognized;
                    // synthesizer.SpeakCompleted += synthesizer_SpeakCompleted;
                    //  sre.RecognizeAsync(RecognizeMode.Multiple);
                    impot();
                }

            }
                catch (Exception ex)
                {
                    say("ERROR SIR");
                    //todo log
                }

            
        }
        public void say(String h)
        {
            synthesizer.Speak(h);
        }
        public Form1()
        {
            GetFacesList();
            try
            {

                say("Importing packages");
                say("Wait a second Sir");
                
              // impot();
               

                videoCapture = new Capture(Config.ActiveCameraIndex);
                videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
                videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 450);
                videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 370);
                // videoCapture = new Capture();
                bgrFrame = videoCapture.QueryFrame();
                Application.Idle += new EventHandler(FrameProduced);
               // databind();
               // impot();
            }
            catch
            {
                say("error");
            }
            synthesizer.SetOutputToDefaultAudioDevice();

            state = true;
            // synthesizer.Speak("hai,I am Jack");
            state = false;
            InitializeComponent();
        }
        public void impot()
        {
            synthesizer.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult);
            
            sre.RequestRecognizerUpdate();
            GrammarBuilder gra = new GrammarBuilder(list);

            Grammar gr = new Grammar(gra);
            sre.LoadGrammar(gr);
            sre.SetInputToDefaultAudioDevice();

            sre.SpeechRecognized += sre_SpeechRecognized;
            synthesizer.SpeakCompleted += synthesizer_SpeakCompleted;
            sre.RecognizeAsync(RecognizeMode.Multiple);
        }
        public void databind(string h)
        {
            try
            {
                conn = "server=localhost;user id=root;database=jack;";
                connect = new MySqlConnection(conn);
                connect.Open();
            }
            catch
            {

            }
            if(h == "Alan Lands")
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = "select question from jack.question";
                cmd.Connection = connect;
                MySqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(dr["question"].ToString());
                }
            }
        }

        private void synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            state = false;

        }

        private void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string question = e.Result.Text;

            if (state == false)
            {
                data(question);
            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            bot = new ChatBot();
            //say("I am at your service sir");
        }
        public void data(string h)
        {
            label4.Text = h;
            state = true;

            //label12.Visible = true;
            switch (h)
            {
                case "hello":
                    {
                        synthesizer.SpeakAsync("hello Sir");
                        label123.Text = "Hello Sir.";
                        break;
                    }
                case "jack are you up":
                    label123.Text = "At your service, Sir";
                    synthesizer.SpeakAsync("At your service");
                    synthesizer.SpeakAsync("Sir");

                    break;
                case "how are you":
                    {
                        label123.Text = "I'm doing Great";
                        synthesizer.SpeakAsync("I'm doing Great");
                        break;
                    }
                case "what is the time":
                    {
                        label123.Text = "Current Time is" + DateTime.Now.ToShortTimeString();
                        synthesizer.SpeakAsync("Current Time is" + DateTime.Now.ToShortTimeString());
                        break;
                    }
                case "open chrome":
                    {
                        label123.Text = "Yes rightaway Sir";
                        synthesizer.SpeakAsync("Yes rightaway Sir");
                        Process.Start("chrome.exe", "http:\\www.google.com");
                        break;
                    }
                case "jack i want to check my email":
                    {
                        label123.Text = "Yes rightaway Sir";
                        synthesizer.SpeakAsync("Yes rightaway Sir");
                        Process.Start("chrome.exe", "https://mail.google.com/mail/u/1/#inbox");
                        break;
                    }
                case "jack i want to check my college mail":
                    {
                        label123.Text = "Yes rightaway Sir";
                        synthesizer.SpeakAsync("Yes rightaway Sir");
                        Process.Start("chrome.exe", "https://mail.google.com/mail/u/0/#inbox");
                        break;
                    }
                case "Do you know me":
                    {
                        sre.SpeechRecognized += sre_SpeechRecognized;
                        
                        videoCapture = new Capture(Config.ActiveCameraIndex);
                        videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
                        videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 450);
                        videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 370);
                       // videoCapture = new Capture();
                        bgrFrame = videoCapture.QueryFrame();
                        Application.Idle += new EventHandler(FrameProduced);
                    }
                    break;
                case "Save me":
                    
                    break;


                case "open youtube":
                    {
                        label123.Text = "Yes rightaway Sir";
                        synthesizer.SpeakAsync("Yes rightaway Sir");
                        Process.Start("chrome.exe", "http:\\www.youtube.com");
                        break;
                    }

                case "thank you":
                    {
                        label123.Text = "No Problem";
                        synthesizer.SpeakAsync("No problem");
                        break;
                    }
                case "hide yourself":
                    {
                        label123.Text = "Okay Sir";
                        synthesizer.SpeakAsync("Okay Sir");
                        this.Hide();
                        break;
                    }
                case "where are you":
                case "jack show yourself":
                    {
                        label123.Text = "Here I am ";
                        synthesizer.SpeakAsync("Here I am");
                        this.Show();
                        break;
                    }

                case "close":
                    {
                        label123.Text = "Okay Sir";
                        synthesizer.SpeakAsync("Okay Sir");
                        Application.Exit();
                        break;
                    }
                case "close chrome":
                    {
                        label123.Text = "Okay Sir";
                        synthesizer.SpeakAsync("Okay Sir");
                        Process[] chromeInstances = Process.GetProcessesByName("chrome");

                        foreach (Process p in chromeInstances)
                            p.Kill();
                        break;
                    }
                case "What is the weather":
                    {
                        label123.Text = "It's pretty hot outside.";
                        synthesizer.SpeakAsync("It's pretty hot inside me.");
                        break;
                    }
                case "wake up daddy is home":
                    {
                        label123.Text = "Welcome Home Sir";
                        synthesizer.SpeakAsync("Welcome Home Sir");
                        this.Show();
                        break;
                    }
                case "which languages do you speak":
                    {
                        label123.Text = "Presently I only understand and speak English.";
                        synthesizer.SpeakAsync("Presently I only understand and speak English.");
                        break;
                    }
                case "Log Off the computer":
                    {
                        label123.Text = "Logging off the System";
                        synthesizer.SpeakAsync("Logging off the System");
                        ExitWindowsEx(0, 0);
                        break;
                    }
                case "Put the computer to sleep":
                    {
                        label123.Text = "Putting the computer to sleep";
                        synthesizer.SpeakAsync("Putting the computer to sleep");
                        SetSuspendState(false, true, true);
                        break;
                    }
                case "shut down the pc":
                    {
                        label123.Text = "Doing a System Shut Down";
                        synthesizer.SpeakAsync("Doing a System Shut Down");
                        Application.Exit();
                        Process.Start("shutdown", "/s /t 0");
                        break;
                    }
                case "Restart the computer":
                    {
                        label123.Text = "Doing a System Restart";
                        synthesizer.SpeakAsync("Doing a System Restart");
                        Application.Exit();
                        Process.Start("shutdown", "/r /t 0");
                        break;
                    }
                case "jack i need to ask you a question that is not on your database":
                    {
                        label123.Text = "Ok,Sir Would you please write down the question for me";
                        say("Ok,Sir Would you please write down the question for me");
                        textBox1.Visible = true;
                        button1.Visible = true;
                        break;
                    }
                case "Alan Lands":
                    {
                        label123.Text = "Hai Sir";
                        synthesizer.SpeakAsync("Hai Sir");
                        break;
                    }


                case "out of the way":

                    say("My apologies");
                    label123.Text = "My apologies";
                    this.Hide();

                    break;
                case "come back":
                    say("Alright?");
                    label123.Text = "Alright";
                    this.Show();
                    break;
                default:
                    {

                        String r = h;
                        String answer = "";
                        answer = bot.getOutput(r);
                        synthesizer.SpeakAsync(answer);
                        label123.Text = answer;

                        break;
                    }
            }


        }



       

        


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                conn = "server=localhost;user id=root;database=jack;";
                connect = new MySqlConnection(conn);
                connect.Open();
                string ques = textBox1.Text;
                string query = "insert into jack.question (question) values('" + ques + "')";
                MySqlCommand cmd = new MySqlCommand(query);
                cmd.Connection = connect;
                cmd.ExecuteReader();
                data(ques);
                textBox1.Visible = false;
                button1.Visible = false;
            }
            catch
            {
                say("Sir there is a problem");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            detectedFace = detectedFace.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            detectedFace.Save(Config.FacePhotosPath + "face" + (faceList.Count + 1) + Config.ImageFileExtension);
            StreamWriter writer = new StreamWriter(Config.FaceListTextFile, true);
            string personName = textBox2.Text;
            writer.WriteLine(String.Format("face{0}:{1}", (faceList.Count + 1), personName));
            writer.Close();
            GetFacesList();
            MessageBox.Show("Succesfull.");
            Application.Restart();
            
                Application.Idle -= new EventHandler(FrameProduced);
           
        }
    }
}
#endregion
#endregion
