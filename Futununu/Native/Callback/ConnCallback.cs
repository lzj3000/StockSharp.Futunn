using Futu.OpenApi;
using Futu.OpenApi.Pb;
using StockSharp.Localization;
using StockSharp.Logging;
using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockSharp.Futunn.Native
{
    /// <summary>
    /// Connected Callback
    /// </summary>
    public class ConnCallback : BaseLogReceiver, FTSPI_Conn
    {
        /// <summary>
        /// Connected
        /// </summary>
        public event Action<bool,string> Connected;
        /// <summary>
        /// Disconnected
        /// </summary>
        public event Action Disconnected;
        /// <summary>
        /// OnInitConnect
        /// </summary>
        /// <param name="client">connect client</param>
        /// <param name="errCode">error code</param>
        /// <param name="desc">if error true </param>
        public void OnInitConnect(FTAPI_Conn client, long errCode, string desc)
        {
            Console.WriteLine("InitConnected");
            
            if (errCode == 0)
            {
                this.AddInfoLog(LocalizedStrings.Connecting);
                Connected?.Invoke(true, desc);
            }
            else {
                Connected?.Invoke(false, desc);
            }
        }
        /// <summary>
        /// OnDisconnect
        /// </summary>
        /// <param name="client">connect client</param>
        /// <param name="errCode">error code</param>
        public void OnDisconnect(FTAPI_Conn client, long errCode)
        {
            this.AddInfoLog(LocalizedStrings.Disconnecting);
            Console.WriteLine("DisConnected");
            Disconnected?.Invoke();
        }
    }
}
