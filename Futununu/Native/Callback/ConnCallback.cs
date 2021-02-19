using Futu.OpenApi;
using Futu.OpenApi.Pb;
using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockSharp.Futunn.Native
{
    public class ConnCallback : FTSPI_Conn
    {
        public event Action<bool,string> Connected;
        public event Action Disconnected;
        public void OnInitConnect(FTAPI_Conn client, long errCode, string desc)
        {
            Console.WriteLine("InitConnected");
            if (errCode == 0)
            {
                Connected?.Invoke(true, desc);
            }
            else {
                Connected?.Invoke(false, desc);
            }
        }

        public void OnDisconnect(FTAPI_Conn client, long errCode)
        {
            Console.WriteLine("DisConnected");
            Disconnected?.Invoke();
        }
    }
}
