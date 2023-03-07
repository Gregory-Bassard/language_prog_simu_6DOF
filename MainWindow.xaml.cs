using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        private DispatcherTimer _timer;

        private const string ORDERS = "LET INC MUL POS_ABS POS_REL ROT_ABS ROT_REL RESET VERIN_ABS VERIN_REL RUN WAIT LABEL GOTO";

        double[] pos = new double[6];

        public MainWindow()
        {
            InitializeComponent();

            orderChecker = new OrderChecker();
            hexapode = new Hexapode();

            pos[0] = hexapode.X;
            pos[1] = hexapode.Y;
            pos[2] = hexapode.Z;
            pos[3] = hexapode.roll;
            pos[4] = hexapode.pitch;
            pos[5] = hexapode.yaw;

            orderChecker.targetPos = pos;

            serialClient = new SerialClient("COM3", 9600);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            serialClient.Open();
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            AffichageVar(cbInfoDisplay.Text);

            if (orderChecker.running && serialClient.GetIsConnected())
            {
                hexapode.CalculPosHexapode();
                SendPos();
                hexapode.Update();
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
            serialClient.SendData(hexapode.GetData()); // envoie des data à l'arduino
        }

        private void CheckCode()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                Parsing(lines[i], i);
            }
        }
        private void Parsing(string line, int index)
        {
            //Verification du 1er mot de la ligne
            string[] words = line.Split(' ');

            orderChecker.OrderCheck(words, index);

        }
        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            orderChecker = new OrderChecker();
            hexapode.ResetPos();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            lines = tbCodeZone.Text.Split('\n');
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
