using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace language_prog_simu_6DOF
{
    internal class SerialClient
    {
        private SerialPort serialPort;
        private string port { get; set; }
        private int portSpeed { get; set; }

        public SerialClient(string port, int portSpeed)
        {
            this.port = port;
            this.portSpeed = portSpeed;
            this.serialPort = new SerialPort(port, portSpeed);
        }
        public void Open() //Ouverture du port
        {
            try
            {
                serialPort.Open();
            }
            catch (System.UnauthorizedAccessException e)
            {
                MessageBox.Show($"Connection Error! {e}", "Alert");
            }
        }
        public void Close() //Fermeture du port
        {
            if (serialPort.IsOpen)
                try
                {
                    serialPort.Close();
                }
                catch (System.UnauthorizedAccessException e)
                {
                    MessageBox.Show($"Connection Error! {e}", "Alert");
                }
        }
        public void SendData(string data)
        {
            serialPort.WriteLine(data);
            //serialPort.DiscardInBuffer();
        }
        public bool GetIsConnected()
        {
            return serialPort.IsOpen;
        }
    }
}
