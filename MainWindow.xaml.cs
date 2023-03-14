
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace language_prog_simu_6DOF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool running = false;

        private OrderChecker orderChecker;
        private Hexapode hexapode;
        private SerialClient serialClient;

        private string[] lines = new string[50];
        private int lineIndex = 0;

        private int nbStep = 10;
        private double speed = 1;

        private int stepCount = 1;

        private DispatcherTimer _timer;
        private double time;

        private const string ORDERS = "LET INC MUL POS_ABS POS_REL ROT_ABS ROT_REL RESET VERIN_ABS VERIN_REL RUN WAIT LABEL GOTO";

        private double[] actPos = new double[6];

        private double[] limitPosPlus = new double[6];
        private double[] limitPosMinus = new double[6];

        double[] deltaPos = new double[6];

        public MainWindow()
        {
            InitializeComponent();

            using (StreamWriter sw = File.CreateText(@"./Debug.csv"))
                sw.WriteLine("time (s);leg0;leg1;leg2;leg3;leg4;leg5;x;y;z;yaw;pitch;roll");

            ConfigIni();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            serialClient.Open();
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            DisplayVar(cbInfoDisplay.Text);
            tbStepSec.Text = nbStep.ToString();

            time += _timer.Interval.TotalSeconds;

            _timer.Interval = TimeSpan.FromMilliseconds(500 / speed);

            if (serialClient.GetIsConnected())
            {
                hexapode.CalculPosHexapode();
                SendPos();
                hexapode.Update();
                Debug.WriteLine($"x : {hexapode.X}, y : {hexapode.Y}, z : {hexapode.Z}, yaw : {hexapode.yaw}, pitch : {hexapode.pitch}, roll : {hexapode.roll}");

                if (running)
                {
                    hexapode.X += deltaPos[0];
                    hexapode.Y += deltaPos[1];
                    hexapode.Z += deltaPos[2];
                    hexapode.yaw += deltaPos[3];
                    hexapode.pitch += deltaPos[4];
                    hexapode.roll += deltaPos[5];

                    if (stepCount == nbStep)
                    {
                        running = false;
                        orderChecker.running = false;
                        stepCount = 1;
                        actPos = hexapode.GetPos();
                        CheckCode();
                    }
                    else
                        stepCount++;
                }
            }
        }

        private void DisplayVar(string? labelName)
        {
            if (string.IsNullOrEmpty(labelName))
                return;
            else
            {
                switch (labelName)
                {
                    case "Variables":
                        if (orderChecker.Variables != null)
                        {
                            List<string> items = new List<string>();
                            foreach (Variable var in orderChecker.Variables)
                            {
                                string item = $"{var.name.Remove(0, 1)} : {var.val}";
                                items.Add(item);
                            }
                            listbInfoData.ItemsSource = items;
                        }
                        break;
                    case "Positions Platforme":
                        if (hexapode != null)
                        {
                            List<string> items = new List<string>()
                            {
                                "x : " + hexapode.X.ToString(),
                                "y : " + hexapode.Y.ToString(),
                                "z : " + hexapode.Z.ToString(),
                                "yaw : " + hexapode.yaw.ToString(),
                                "pitch : " + hexapode.pitch.ToString(),
                                "roll : " + hexapode.roll.ToString()
                            };
                            listbInfoData.ItemsSource = items;
                        }
                        break;
                    case "Target Positions":
                        if (orderChecker.targetPos != null)
                        {
                            List<string> items = new List<string>()
                            {
                                "x : " + orderChecker.targetPos[0].ToString(),
                                "y : " + orderChecker.targetPos[1].ToString(),
                                "z : " + orderChecker.targetPos[2].ToString(),
                                "yaw : " + orderChecker.targetPos[3].ToString(),
                                "pitch : " + orderChecker.targetPos[4].ToString(),
                                "roll : " + orderChecker.targetPos[5].ToString()
                            };
                            listbInfoData.ItemsSource = items;
                        }
                        break;
                    case "Legs":
                        if (hexapode.lengthVer != null)
                        {
                            List<string> items = new List<string>();
                            for (int i = 0; i < hexapode.lengthVer.Count(); i++)
                            {
                                items.Add($"leg {i} : {hexapode.lengthVer[i]:0.000}");
                            }
                            listbInfoData.ItemsSource = items;
                        }
                        break;
                }
            }
        }
        public void SendPos()
        {
            string data = hexapode.GetData();
            if (data != null && data != "0.000,0.000,0.000,0.000,0.000,0.000")
            {
                using (StreamWriter sw = File.AppendText(@"./Debug.csv"))
                    sw.WriteLine($"{time};{data.Replace(",", ";")};{hexapode.X};{hexapode.Y};{hexapode.Z};{hexapode.yaw};{hexapode.pitch};{hexapode.roll}");
                serialClient.SendData(data); // envoie des data à l'arduino
            }
        }
        private void Interpolate(double[] actualPos, double[] targetPos, int nbStep)
        {
            for (int i = 0;i < 6; i++)
                deltaPos[i] = (targetPos[i] - actualPos[i]) / nbStep;
        }
        private void CheckCode()
        {
            while (!orderChecker.running && lineIndex < lines.Count())
            {
                if (lines[lineIndex][0] != '#')
                    Parsing(lines[lineIndex]);
                lineIndex++;
            }
            if (orderChecker.running)
            {
                var result = MessageBoxResult.Yes;
                if (orderChecker.NormalizeTargetPos(limitPosPlus, limitPosMinus))
                    result = MessageBox.Show($"Warning : Work area overrun.\r Do you want to stop the program ?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    Interpolate(actPos, orderChecker.targetPos, nbStep);
                    running = true;
                }
                else
                    Restart();
            }
        }
        private void Parsing(string line)
        {
            //Verification du 1er mot de la ligne
            string[] words = line.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty).Split(' ');
            
            foreach (string word in words)
                if (word.Contains(" ") && word.Length <= 1 || word == string.Empty && word.Length <= 1)
                    words = words.Where(w => w != word).ToArray();
                //else if (word.Contains(" ") && word.Length > 1)
                //    word.Replace(" ", string.Empty);

            orderChecker.OrderCheck(words, lineIndex);

            if (orderChecker.curIndex != lineIndex)
            {
                lineIndex = orderChecker.curIndex;
            }
        }
        private void ConfigIni()
        {
            string[] confLines = System.IO.File.ReadAllLines(@"./Config.ini");

            int portSpeed = 0;
            string port = "";
            double rayVerBase = 0.00, alphaBase = 0.00, betaBase = 0.00, rayVerPlat = 0.00, alphaPlat = 0.00, betaPlat = 0.00, height = 0.00;
            double[] startPos = new double[6];

            foreach (string line in confLines)
            {
                if (line.Contains("="))
                {
                    string[] result = line.Split("=");
                    switch (result[0])
                    {
                        case "rayVerBase":
                            rayVerBase = Convert.ToDouble(result[1]);
                            break;
                        case "angVerBase":
                            alphaBase = Convert.ToDouble(result[1]);
                            betaBase = 2 * Math.PI / 3 - alphaBase;
                            break;
                        case "rayVerPlat":
                            rayVerPlat = Convert.ToDouble(result[1]);
                            break;
                        case "angVerPlat":
                            alphaPlat = Convert.ToDouble(result[1]);
                            betaPlat = 2 * Math.PI / 3 - alphaPlat;
                            break;
                        case "height":
                            height = Convert.ToDouble(result[1]);
                            break;
                        case "startPos":
                            for (int i = 0; i < 6; i++)
                            {
                                actPos[i] = double.Parse(result[1].Split(',')[i]);
                                startPos[i] = double.Parse(result[1].Split(',')[i]);
                            }
                                
                            break;
                        case "limitPosPlus":
                            for (int i = 0; i < 6; i++)
                                limitPosPlus[i] = double.Parse(result[1].Split(',')[i]);
                            break;
                        case "limitPosMinus":
                            for (int i = 0; i < 6; i++)
                                limitPosMinus[i] = double.Parse(result[1].Split(',')[i]);
                            break;
                        case "portSpeed":
                            portSpeed = int.Parse(result[1]);
                            break;
                        case "port":
                            port = result[1];
                            break;
                    }
                }
            }
            Init(portSpeed, port, rayVerBase, alphaBase, betaBase, rayVerPlat, alphaPlat, betaPlat, height, startPos);
        }
        private void Init(int portSpeed, string port, double rayVerBase, double alphaBase, double betaBase, double rayVerPlat, double alphaPlat, double betaPlat, double height, double[] startPos)
        {
            orderChecker = new OrderChecker();
            hexapode = new Hexapode(startPos);

            hexapode.rayVerBase = rayVerBase;
            hexapode.alphaBase = alphaBase;
            hexapode.betaBase = betaBase;
            hexapode.rayVerPlat = rayVerPlat;
            hexapode.alphaPlat = alphaPlat;
            hexapode.betaPlat = betaPlat;
            hexapode.height = height;
            hexapode.centreRotation.Z = height;

            orderChecker.targetPos = startPos;

            serialClient = new SerialClient(port, portSpeed);
        }
        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            Restart();
        }

        private void Restart()
        {
            serialClient.Close();
            _timer.Stop();
            ConfigIni();
            _timer.Start();
            serialClient.Open();
            //tbCodeZone.Text = ""; //a voire
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            lines = tbCodeZone.Text.Split('\n');
            lineIndex = 0;
            orderChecker.labels = new List<Tuple<string, int>>();
            for (int i = 0; i < lines.Length; i++)
                orderChecker.CreateLabel(lines[i].Split(' '), i);
            CheckCode();
        }

        private void slSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speed = e.NewValue;
        }

        private void btnDecrease_Click(object sender, RoutedEventArgs e)
        {
            nbStep--;
        }

        private void btnIncrease_Click(object sender, RoutedEventArgs e)
        {
            nbStep++;
        }

        private void cbInfoDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void miSave_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void miSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void SaveAs()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Sim Files";

            Directory.CreateDirectory(path);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "simulator file(*.sim)|*.sim";
            dlg.InitialDirectory = path;
            dlg.DefaultExt = ".sim";
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, tbCodeZone.Text);
            }
        }

        private void miOpen_Click(object sender, RoutedEventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Sim Files";

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "simulator file(*.sim)|*.sim";
            ofd.Multiselect = false;
            ofd.InitialDirectory = path;
            if (ofd.ShowDialog() == true)
            {
                tbCodeZone.Text = File.ReadAllText(ofd.FileName);
            }
        }

        private void miStep_Click(object sender, RoutedEventArgs e)
        {

        }

        private void miStepLine_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
