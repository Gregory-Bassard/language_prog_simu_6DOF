using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace language_prog_simu_6DOF
{
    internal class OpcUaClient
    {
        private OpcClient client;

        public OpcUaClient(string serverIp, string serverPort, string serverPath)
        {
            client = new OpcClient($"opc.tcp://{serverIp}:{serverPort}/{serverPath}");
        }

        public void Connect()
        {
            if (client != null)
            {
                client.Connect();

                if (client.State == OpcClientState.Connected)
                    Debug.WriteLine("Connected to OPC-UA server !");
                else
                    Debug.WriteLine("Unable to connect to OPC-UA server !");
            }
        }
        public void Disconnect()
        {
            if(client != null)
                client.Disconnect();
        }
        public void WriteObject(int objId, int newValue)
        {
            OpcNodeId obj = new OpcNodeId(objId);

            var currentValue = client.ReadNode(obj);
            Debug.WriteLine("Actual value : " + currentValue);

            client.WriteNode(obj, newValue);

            var updatedValue = client.ReadNode(obj);
            Debug.WriteLine("New value : " + updatedValue);
        }
    }
}
