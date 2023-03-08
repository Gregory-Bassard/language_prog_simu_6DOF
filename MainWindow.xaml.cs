using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private bool error = false;

        private OrderChecker orderChecker;
        private Hexapode hexapode;
        private SerialClient serialClient;

        private string[] lines = new string[50];
        private int lineIndex = 0;

        private DispatcherTimer _timer;
        private double time;

        private const string ORDERS = "LET INC MUL POS_ABS POS_REL ROT_ABS ROT_REL RESET VERIN_ABS VERIN_REL RUN WAIT LABEL GOTO";

        double[] pos = new double[6];

        double[] limitPosPlus = new double[6];
        double[] limitPosMinus = new double[6];

        public MainWindow()
        {
            InitializeComponent();

            using (StreamWriter sw = File.CreateText(@"./Debug.csv"))
                sw.WriteLine("time (s);leg0;leg1;leg2;leg3;leg4;leg5;x;y;z;yaw;pitch;roll");

            ConfigIni();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            serialClient.Open();
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            AffichageVar(cbInfoDisplay.Text);

            time += _timer.Interval.TotalSeconds;

            if (serialClient.GetIsConnected())
            {
                hexapode.CalculPosHexapode();
                SendPos();
                hexapode.Update();
                Debug.WriteLine($"x : {hexapode.X}, y : {hexapode.Y}, z : {hexapode.Z}, yaw : {hexapode.yaw}, pitch : {hexapode.pitch}, roll : {hexapode.roll}");

                if (orderChecker.running) //TODO : savoir quand la plateforme à atteint sa position target
                {
                    orderChecker.running = false;
                    CheckCode();
                }
            }
        }

        private void AffichageVar(string? labelName)
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
            using (StreamWriter sw = File.AppendText(@"./Debug.csv"))
                sw.WriteLine($"{time};{data.Replace(",", ";")};{hexapode.X};{hexapode.Y};{hexapode.Z};{hexapode.yaw};{hexapode.pitch};{hexapode.roll}");
            serialClient.SendData(data); // envoie des data à l'arduino
        }

        private void CheckCode()
        {
            while (!orderChecker.running && lineIndex < lines.Count())
            {
                Parsing(lines[lineIndex], lineIndex);
                lineIndex++;
            }
            if (orderChecker.running)
            {
                double[] targetPos = orderChecker.GetTargetPos(limitPosPlus, limitPosMinus);
                hexapode.X = targetPos[0];
                hexapode.Y = targetPos[1];
                hexapode.Z = targetPos[2];
                hexapode.yaw = targetPos[3];
                hexapode.pitch = targetPos[4];
                hexapode.roll = targetPos[5];
            }
        }
        private void Parsing(string line, int index)
        {
            //Verification du 1er mot de la ligne
            string[] words = line.Split(' ');

            orderChecker.OrderCheck(words, index);
        }
        private void ConfigIni()
        {
            string[] confLines = System.IO.File.ReadAllLines(@"./Config.ini");

            int portSpeed = 0;
            string port = "";
            double rayVerBase = 0.00, alphaBase = 0.00, betaBase = 0.00, rayVerPlat = 0.00, alphaPlat = 0.00, betaPlat = 0.00, height = 0.00;

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
            Init(portSpeed, port, rayVerBase, alphaBase, betaBase, rayVerPlat, alphaPlat, betaPlat, height);
        }
        private void Init(int portSpeed, string port, double rayVerBase, double alphaBase, double betaBase, double rayVerPlat, double alphaPlat, double betaPlat, double height)
        {
            orderChecker = new OrderChecker();
            hexapode = new Hexapode(0, 0, 0, 0, 0, 0);
            pos[0] = hexapode.X;
            pos[1] = hexapode.Y;
            pos[2] = hexapode.Z;
            pos[3] = hexapode.roll;
            pos[4] = hexapode.pitch;
            pos[5] = hexapode.yaw;

            hexapode.rayVerBase = rayVerBase;
            hexapode.alphaBase = alphaBase;
            hexapode.betaBase = betaBase;
            hexapode.rayVerPlat = rayVerPlat;
            hexapode.alphaPlat = alphaPlat;
            hexapode.betaPlat = betaPlat;
            hexapode.height = height;
            hexapode.centreRotation.Z = height;

            orderChecker.targetPos = pos;

            serialClient = new SerialClient(port, portSpeed);
        }
        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            ConfigIni();
            tbCodeZone.Text = "";
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            lines = tbCodeZone.Text.Split('\n');
            lineIndex = 0;
            CheckCode();
        }

        private void slSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //TODO : Speed Slider Value Change
        }

        private void btnDecrease_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnIncrease_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cbInfoDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void miSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void miOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void miStep_Click(object sender, RoutedEventArgs e)
        {

        }

        private void miStepLine_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
