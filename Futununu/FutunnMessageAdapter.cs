﻿using Ecng.Common;
using Futu.OpenApi;
using StockSharp.Futunn.Native;
using StockSharp.Messages;
using System;

namespace StockSharp.Futunn
{

    /// <summary>
    /// Futu证券消息适配器,用于交易中国市场股票
    /// </summary>
    /// <include file='FTAPIChannel.dll' path='[@name=""]'/>
    /// <remarks></remarks>
    [OrderCondition(typeof(FutunnOrderCondition))]
    public partial class FutunnMessageAdapter : MessageAdapter
    {
      
        /// <summary>
		/// Default value for <see cref="MessageAdapter.HeartbeatInterval"/>.
		/// </summary>
		public static readonly TimeSpan DefaultHeartbeatInterval = TimeSpan.FromSeconds(1);
        /// <summary>
		/// Initializes a new instance of the <see cref="FutunnMessageAdapter"/>.
		/// </summary>
		/// <param name="transactionIdGenerator">Transaction id generator.</param>
        public FutunnMessageAdapter(IdGenerator transactionIdGenerator) : base(transactionIdGenerator)
        {
            HeartbeatInterval = DefaultHeartbeatInterval;

            this.AddMarketDataSupport();
            this.AddTransactionalSupport();

            this.AddSupportedMarketDataType(DataType.Ticks);
            this.AddSupportedMarketDataType(DataType.MarketDepth);
            this.AddSupportedMarketDataType(DataType.Level1);
            this.AddSupportedMarketDataType(DataType.OrderLog);

            this.AddSupportedResultMessage(MessageTypes.SecurityLookup);
            this.AddSupportedResultMessage(MessageTypes.PortfolioLookup);
            this.AddSupportedResultMessage(MessageTypes.OrderStatus);
            FTAPI.Init();
        }
       
        protected override bool OnSendInMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageTypes.Connect:
                    {
                        onConnect();
                        break;
                    }
                case MessageTypes.Disconnect:
                    {
                        onDisconnec();
                        break;
                    }
                case MessageTypes.Reset:
                    {
                        break;
                    }
                case MessageTypes.MarketData:
                    {
                        ProcessMarketData((MarketDataMessage)message);
                        break;
                    }
                case MessageTypes.SecurityLookup:
                    {
                        ProcessSecurityLookup((SecurityLookupMessage)message);
                        break;
                    }
                case MessageTypes.OrderStatus:
                    {
                        ProcessOrderStatus((OrderStatusMessage)message);
                        break;
                    }

                case MessageTypes.OrderRegister:
                    {
                        ProcessOrderRegister((OrderRegisterMessage)message);
                        break;
                    }

                case MessageTypes.OrderCancel:
                    {
                        ProcessOrderCancel((OrderCancelMessage)message);
                        break;
                    }
                case MessageTypes.PortfolioLookup:
                    {
                        ProcessPortfolioLookup((PortfolioLookupMessage)message);
                        break;
                    }
                case MessageTypes.Time:
                    {
                        break;
                    }
                default:
                    return false;
            }
            return true;
        }
        FTAPI_Qot client;
        MarketData market;
        Transaction transaction;
        private void onConnect()
        {
            if (OpendIP.IsEmpty())
                throw new InvalidOperationException("OpendIP is Empty.");
            if (OpendPort == 0)
                throw new InvalidOperationException("OpendPort is Empty.");
            if (this.IsTransactional())
            {
                if (Login.IsEmpty())
                    throw new InvalidOperationException("Login is Empty.");
                if (Password.IsEmpty())
                    throw new InvalidOperationException("Password is Empty.");
            }
            client = new FTAPI_Qot();
            var conn = new ConnCallback();
            conn.Connected += Conn_Connected;
            conn.Disconnected += Conn_Disconnected;
            client.SetConnCallback(conn);
            client.InitConnect(OpendIP, OpendPort, false);
        }
        private void onDisconnec()
        {
            UnSubscribeMarketInfo();
            if (market != null)
            {
                market.Error -= On_Error;
                market = null;
            }
            if (transaction != null) {
                transaction.Error -= On_Error;
                transaction = null;
            }

            if (client != null)
                client.Close();
        }

        private void Conn_Disconnected()
        {
            SendOutDisconnectMessage(true);
        }
        private void Conn_Connected(bool arg1, string arg2)
        {
            if (arg1)
            {
                if (market == null)
                {
                    market = new MarketData(OpendIP, OpendPort);
                    market.Error += On_Error;
                }
                if (transaction == null)
                {
                    transaction = new Transaction(OpendIP, OpendPort, Login, Password.ToString());
                    transaction.Error += On_Error;
                }
                SubscribeMarketInfo();
                SendOutMessage(new ConnectMessage());
            }
            else
                SendOutError(arg2);
        }

        private void On_Error(Exception obj)
        {
            SendOutError(obj.Message);
        }

      
    }
}
