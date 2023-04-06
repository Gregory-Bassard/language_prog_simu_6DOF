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

        public OpcUaClient(string serverPath)
        {
            client = new OpcClient($"{serverPath}");
        }

        public bool Connect()
        {
            if (client != null)
            {
                client.Connect();

                if (client.State == OpcClientState.Connected)
                {
                    Debug.WriteLine("Connected to OPC-UA server !");
                    return true;
                }
                Debug.WriteLine("Unable to connect to OPC-UA server !");
                return false;
            }
            return false;
        }
        public void Disconnect()
        {
            if (client != null)
                client.Disconnect();
        }
        public void WriteObject(string objId, int nameSpace, string newValue)
        {
            OpcNodeId obj = new OpcNodeId(objId, nameSpace);
            client.WriteNode(obj, newValue);
        }
        public void WriteObject(string objId, int nameSpace, int newValue)
        {
            OpcNodeId obj = new OpcNodeId(objId, nameSpace);
            client.WriteNode(obj, newValue);
        }
        public void WriteObject(string objId, int nameSpace, float newValue)
        {
            OpcNodeId obj = new OpcNodeId(objId, nameSpace);
            client.WriteNode(obj, newValue);
        }
        public void WriteObject(string objId, int nameSpace, float[] newValue)
        {
            OpcNodeId obj = new OpcNodeId(objId, nameSpace);
            client.WriteNode(obj, newValue);
        }
        public void WriteObject(string objId, int nameSpace, bool newValue)
        {
            OpcNodeId obj = new OpcNodeId(objId, nameSpace);
            client.WriteNode(obj, newValue);
        }
    }
}
