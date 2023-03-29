
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
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
using Label = System.Windows.Controls.Label;

namespace language_prog_simu_6DOF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string pathFile = "";

        public DateTime dt1 = DateTime.Now;
        private bool running = false;

        private bool stepMode = false;

        private bool usingSerial = false;
        private bool usingOpcUa = false;

        private OrderChecker orderChecker;
        private Hexapode hexapode;
        private OpcUaClient opcUaClient;
        private SerialClient serialClient;

        private string[] lines = new string[50];
        private int lineIndex = 0;

        private int nbStep = 2;
        private double speed = 1;

        private double timerTick = 100;

        private int stepCount = 1;

        private DispatcherTimer _timer;

        private double[] actPos = new double[6];

        private double[] limitPosPlus = new double[6];
        private double[] limitPosMinus = new double[6];

        private double legslimitPlus = 0.00;
        private double legslimitMinus = 0.00;

        double[] deltaPos = new double[6];

        public MainWindow()
        {
            InitializeComponent();

            using (StreamWriter sw = File.CreateText(@"./Debug.csv"))
                sw.WriteLine("time (s);leg0;leg1;leg2;leg3;leg4;leg5;x;y;z;yaw;pitch;roll");

            ConfigIni();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            serialClient.Open();
        }
        private void ConfigIni()
        {
            string[] confLines = System.IO.File.ReadAllLines(@"./Config.ini");

            int portSpeed = 0;
            string port = "", serverIp = "", serverPort = "", serverPath = "";
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
                        case "legsLimitPlus":
                            legslimitPlus = double.Parse(result[1]);
                            break;
                        case "legsLimitMinus":
                            legslimitMinus = double.Parse(result[1]);
                            break;
                        case "serverIp":
                            serverIp = result[1];
                            break;
                        case "serverPort":
                            serverPort = result[1];
                            break;
                        case "serverPath":
                            serverPath = result[1];
                            break;
                        case "usingOpcUaConnection":
                            usingOpcUa = bool.Parse(result[1]);
                            break;
                        case "portSpeed":
                            portSpeed = int.Parse(result[1]);
                            break;
                        case "port":
                            port = result[1];
                            break;
                        case "usingSerialConnection":
                            usingSerial = bool.Parse(result[1]);
                            break;
                    }
                }
            }
            Init(portSpeed, port, rayVerBase, alphaBase, betaBase, rayVerPlat, alphaPlat, betaPlat, height, startPos, serverIp, serverPort, serverPath);
        }
        private void Init(int portSpeed, string port, double rayVerBase, double alphaBase, double betaBase, double rayVerPlat, double alphaPlat, double betaPlat, double height, double[] startPos, string serverIp, string serverPort, string serverPath)
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

            for (int i = 0; i < 6; i++)
            {
                orderChecker.targetPos[i] = startPos[i];
                orderChecker.initPos[i] = startPos[i];
            }
            
            if (usingOpcUa)
                opcUaClient = new OpcUaClient(serverIp, serverPort, serverPath);
            if (usingSerial)
                serialClient = new SerialClient(port, portSpeed);
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
                            string delta = "\x2206";
                            List<string> items = new List<string>()
                            {
                                "x : " + orderChecker.targetPos[0].ToString() + " --> " + "\x2206" + (orderChecker.targetPos[0]-hexapode.GetPos()[0]).ToString(),
                                "y : " + orderChecker.targetPos[1].ToString() + " --> " + "\x2206" + (orderChecker.targetPos[1]-hexapode.GetPos()[1]).ToString(),
                                "z : " + orderChecker.targetPos[2].ToString() + " --> " + "\x2206" + (orderChecker.targetPos[2]-hexapode.GetPos()[2]).ToString(),
                                "yaw : " + orderChecker.targetPos[3].ToString() + " --> " + "\x2206" + (orderChecker.targetPos[3]-hexapode.GetPos()[3]).ToString(),
                                "pitch : " + orderChecker.targetPos[4].ToString() + " --> " + "\x2206" + (orderChecker.targetPos[4]-hexapode.GetPos()[4]).ToString(),
                                "roll : " + orderChecker.targetPos[5].ToString() + " --> " + "\x2206" + (orderChecker.targetPos[5]-hexapode.GetPos()[5]).ToString()
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
        private void _timer_Tick(object? sender, EventArgs e)
        {
            DisplayVar(cbInfoDisplay.Text);
            tbStepSec.Text = nbStep.ToString();

            _timer.Interval = TimeSpan.FromMilliseconds(timerTick / speed);

            if (orderChecker.error)
                Restart();

            if (btnRun.Content.ToString() == "Next")
                foreach (Label label in spLabelsList.Children)
                    if (label.Name == $"lb{lineIndex}")
                        label.Background = Brushes.Beige;
                    else
                        label.Background = Brushes.Transparent;

            if (running)
            {
                if (stepMode)
                {
                    lbStepCounter.Content = $"{stepCount - 1}/{nbStep * orderChecker.runningTime} Steps";
                    btnRun.Content = "Next Step";
                }
                else
                {
                    hexapode.X += deltaPos[0];
                    hexapode.Y += deltaPos[1];
                    hexapode.Z += deltaPos[2];
                    hexapode.yaw += deltaPos[3];
                    hexapode.pitch += deltaPos[4];
                    hexapode.roll += deltaPos[5];
                    Debug.WriteLine(DateTime.Now - dt1);

                    if (stepCount == nbStep * orderChecker.runningTime)
                    {
                        running = false;
                        orderChecker.running = false;
                        stepCount = 1;
                        actPos = hexapode.GetPos();
                        if (btnRun.Content.ToString() == "Run")
                            CheckCode();
                        else if (btnRun.Content.ToString() == "Next" && lineIndex >= lines.Count())
                            Restart();
                    }
                    else
                        stepCount++;
                }
            }

            hexapode.CalculPosHexapode();
            SendPos();
            hexapode.Update();
            //Debug.WriteLine($"x : {hexapode.X}, y : {hexapode.Y}, z : {hexapode.Z}, yaw : {hexapode.yaw}, pitch : {hexapode.pitch}, roll : {hexapode.roll}");
        }
        private void SendPos()
        {
            string data = hexapode.GetData();
            bool flag = false;
            StringBuilder sb = new StringBuilder();

            //TODO : Test de data
            double[] legs = new double[6];
            for (int i = 0; i < 6; i++)
                legs[i] = double.Parse(data.Split(',')[i]);

            for (int i = 0; i < 6; i++)
            {
                if (legs[i] > legslimitPlus)
                {
                    legs[i] = legslimitPlus;
                    flag = true;
                }
                else if (legs[i] < legslimitMinus)
                {
                    legs[i] = legslimitMinus;
                    flag = true;
                }
                sb.Append(legs[i].ToString());
                if (i != 5)
                    sb.Append(',');
            }
            data = sb.ToString();
            Debug.WriteLine(data);

            if (flag)
            {
                var result = MessageBoxResult.No;
                result = MessageBox.Show($"Warning : Work area overrun.\r Do you want to stop the program ?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    SendData(data);
                }
                else
                    Restart();
            }
            else
            {
                SendData(data);
            }
        }
        private void SendData(string data)
        {
            if (data != null && data != "0.000,0.000,0.000,0.000,0.000,0.000")
            {
                using (StreamWriter sw = File.AppendText(@"./Debug.csv"))
                    sw.WriteLine($"{DateTime.Now - dt1};{data.Replace(",", ";")};{hexapode.X};{hexapode.Y};{hexapode.Z};{hexapode.yaw};{hexapode.pitch};{hexapode.roll}");
                if (usingSerial)
                    serialClient.SendData(data); // envoie des data à l'arduino
                if (usingOpcUa)
                {
                    //TODO :envoie des données
                }
            }
        }
        private void Interpolate(double[] actualPos, double[] targetPos, int nbStep, int time)
        {
            for (int i = 0;i < 6; i++)
                deltaPos[i] = (targetPos[i] - actualPos[i]) / (nbStep * time);
            _timer.Interval = TimeSpan.FromMilliseconds(timerTick = 1000 / nbStep);

        }
        private void CheckCode()
        {
            while (!orderChecker.running && lineIndex < lines.Count() && lines[lineIndex] != null)
            {
                if (lines[lineIndex] != "")
                    if (lines[lineIndex][0] != '#')
                        Parsing(lines[lineIndex]);
                lineIndex++;

            }
            if (orderChecker.running)
            {
                var result = MessageBoxResult.No;
                if (orderChecker.NormalizeTargetPos(limitPosPlus, limitPosMinus))
                    result = MessageBox.Show($"Warning : Work area overrun.\r Do you want to stop the program ?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    Interpolate(actPos, orderChecker.targetPos, nbStep, orderChecker.runningTime);
                    dt1 = DateTime.Now;
                    running = true;
                }
                else
                    Restart();
            }
        }
        private void CheckDebugCode()
        {
            if (!orderChecker.running && lineIndex < lines.Count() && lines[lineIndex] != null)
            {
                try
                {
                    while (lines[lineIndex][0] == '#')
                        lineIndex++;
                    Parsing(lines[lineIndex]);
                }
                catch { }
            }
            if (orderChecker.running)
            {
                var result = MessageBoxResult.No;
                if (orderChecker.NormalizeTargetPos(limitPosPlus, limitPosMinus))
                    result = MessageBox.Show($"Warning : Work area overrun.\r Do you want to stop the program ?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    Interpolate(actPos, orderChecker.targetPos, nbStep, orderChecker.runningTime);
                    dt1 = DateTime.Now;
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
        private void SaveAs(string path, string filter, string ext, bool csv = false)
        {
            Directory.CreateDirectory(path);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = filter + "|*" + ext;
            dlg.InitialDirectory = path;
            dlg.DefaultExt = ext;
            dlg.FileName = DateTime.Now.ToString("MM-dd-yyyy HH-mm");
            if (dlg.ShowDialog() == true)
            {
                if (csv)
                    using (StreamReader sr = File.OpenText(@"./Debug.csv"))
                        File.WriteAllText(dlg.FileName, sr.ReadToEnd());
                else
                {
                    File.WriteAllText(dlg.FileName, tbCodeZone.Text);
                    pathFile = dlg.FileName;
                }
            }
        }
        private void Restart()
        {
            if(serialClient != null)
                serialClient.Close();
            _timer.Stop();
            ConfigIni();
            _timer.Start();
            if (serialClient != null)
                serialClient.Open();
            deltaPos = new double[6];
            running = false;
            stepCount = 1;
            if (stepMode)
            {
                btnRun.Content = "Run Step Mode";
                lbStepCounter.Content = $"{stepCount - 1}/{nbStep * orderChecker.runningTime} Steps";
            }
            else
            {
                btnRun.Content = "Run";
                lbStepCounter.Visibility = Visibility.Hidden;
            }

            tbCodeZone.Visibility = Visibility.Visible;
            spLabelsList.Visibility = Visibility.Hidden;

            //tbCodeZone.Text = ""; //a voire
            miStep.IsEnabled = true;
            miStepLine.IsEnabled = true;
        }
        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            Restart();
        }
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (btnRun.Content.ToString() == "Run" || btnRun.Content.ToString() == "Run Step Mode")
            {
                lines = tbCodeZone.Text.Split('\n');
                lineIndex = 0;
                orderChecker.labels = new List<Tuple<string, int>>();
                for (int i = 0; i < lines.Length; i++)
                    orderChecker.CreateLabel(lines[i].Split(' '), i);
                CheckCode();
            }
            else if (btnRun.Content.ToString() == "Next")
            {
                CheckDebugCode();
                if (lineIndex + 1 >= lines.Length && !running)
                {
                    btnRun.Content = "Run";

                    tbCodeZone.Visibility = Visibility.Visible;
                    spLabelsList.Visibility = Visibility.Hidden;

                    miStep.IsEnabled = true;
                    miStepLine.IsEnabled = true;
                }
                else
                    lineIndex++;
            }
            else if (btnRun.Content.ToString() == "Next Step" && running)
            {
                hexapode.X += deltaPos[0];
                hexapode.Y += deltaPos[1];
                hexapode.Z += deltaPos[2];
                hexapode.yaw += deltaPos[3];
                hexapode.pitch += deltaPos[4];
                hexapode.roll += deltaPos[5];
                Debug.WriteLine(DateTime.Now - dt1);

                if (stepCount == nbStep * orderChecker.runningTime)
                {
                    running = false;
                    orderChecker.running = false;
                    stepCount = 1;
                    actPos = hexapode.GetPos();
                    if (lineIndex < lines.Count())
                        CheckCode();
                    else
                    {
                        stepMode = false;
                        miStep.IsChecked = false;
                        miStepLine.IsEnabled = true;
                        lbStepCounter.Visibility = Visibility.Hidden;
                        btnRun.Content = "Run";
                    }
                }
                else
                    stepCount++;

                hexapode.CalculPosHexapode();
                SendPos();
                hexapode.Update();

            }
        }
        private void slSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speed = e.NewValue;
        }
        private void btnDecrease_Click(object sender, RoutedEventArgs e)
        {
            nbStep--;
            if (nbStep < 1)
                nbStep = 1;
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
            if (File.Exists(pathFile))
            {
                using (StreamWriter sw = File.CreateText(pathFile))
                {
                    foreach (var line in tbCodeZone.Text.Split('\n'))
                        sw.WriteLine(line.ToString());
                }
            }
            else
            {
                string filter = "simulator file(*.sim)";
                string ext = ".sim";
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Sim Files";
                SaveAs(path, filter, ext);
            }
        }
        private void miSaveAs_Click(object sender, RoutedEventArgs e)
        {
            string filter = "simulator file(*.sim)";
            string ext = ".sim";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Sim Files";
            SaveAs(path, filter, ext);
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
                pathFile = ofd.FileName;
            }
        }
        private void miExportCSV_Click(object sender, RoutedEventArgs e)
        {
            string filter = "CSV (*.csv)";
            string ext = ".csv";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Sim Files";
            SaveAs(path, filter, ext, true);
        }
        private void miStep_Click(object sender, RoutedEventArgs e)
        {
            stepMode = !stepMode;

            miStep.IsChecked = stepMode;

            if (miStep.IsChecked)
            {
                lbStepCounter.Visibility = Visibility.Visible;
                btnRun.Content = "Run Step Mode";
                miStepLine.IsEnabled = false;
            }
            else
            {
                lbStepCounter.Visibility = Visibility.Hidden;
                btnRun.Content = "Run";
                miStepLine.IsEnabled = true;
            }
        }
        private void miStepLine_Click(object sender, RoutedEventArgs e)
        {
            btnRun.Content = "Next";
            lines = tbCodeZone.Text.Split('\n');

            tbCodeZone.Visibility = Visibility.Hidden;
            spLabelsList.Visibility = Visibility.Visible;

            spLabelsList.Children.Clear();

            for (int i = 0; i < lines.Length; i++)
            {
                Label label = new Label();
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.VerticalAlignment = VerticalAlignment.Top;
                label.Width = 300;
                label.Height = 25;

                label.Name = $"lb{i}";
                label.Content = lines[i];
                spLabelsList.Children.Add(label);
            }

            lineIndex = 0;
            orderChecker.labels = new List<Tuple<string, int>>();
            for (int i = 0; i < lines.Length; i++)
                orderChecker.CreateLabel(lines[i].Split(' '), i);
            miStep.IsEnabled = false;
            miStepLine.IsEnabled = false;
            CheckDebugCode();
        }
        private void CtrShortcut_S(object sender, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(pathFile))
            {
                using (StreamWriter sw = File.CreateText(pathFile))
                {
                    foreach (var line in tbCodeZone.Text.Split('\n'))
                        sw.WriteLine(line.ToString().Trim());
                }
            }
            else
            {
                string filter = "simulator file(*.sim)";
                string ext = ".sim";
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Sim Files";
                SaveAs(path, filter, ext);
            }
        }
        private void CtrShortcut_O(object sender, ExecutedRoutedEventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Sim Files";

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "simulator file(*.sim)|*.sim";
            ofd.Multiselect = false;
            ofd.InitialDirectory = path;
            if (ofd.ShowDialog() == true)
            {
                tbCodeZone.Text = File.ReadAllText(ofd.FileName);
                pathFile = ofd.FileName;
            }
        }
    }
}