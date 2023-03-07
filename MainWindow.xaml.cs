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
        private bool started = false;
        private bool error = false;

        private OrderChecker orderChecker;

        private string[] lines = new string[50];

        private DispatcherTimer _timer;

        private const string ORDERS = "LET INC MUL POS_ABS POS_REL ROT_ABS ROT_REL RESET VERIN_ABS VERIN_REL RUN WAIT LABEL GOTO";



        public MainWindow()
        {
            InitializeComponent();

            orderChecker = new OrderChecker();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
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

            if (started)
            {

            }
        }
        private void CheckCode()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                Parsing(lines[i], i);
            }

            //verifie si il y a eu une erreur dans l'analyse du code
            if (!error)
            {

            }
        }
        private void Parsing(string line, int index)
        {
            //Verification du 1er mot de la ligne
            string[] words = line.Split(' ');

            orderChecker.OrderCheck(words, index);

        }
        public void Restart()
        {
            orderChecker = new OrderChecker();
        }
        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            started = false;
            Restart();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            started = true;
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
