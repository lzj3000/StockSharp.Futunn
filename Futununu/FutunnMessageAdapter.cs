using Ecng.Common;
using Futu.OpenApi;
using StockSharp.Futunn.Native;
using StockSharp.Messages;
using System;
using System.Collections.Generic;

namespace StockSharp.Futunn
{
   
    /// <summary>
    /// Futu证券消息适配器,用于交易中国市场股票
    /// </summary>
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
            FutunnResources.LoadFTAPI();
            HeartbeatInterval = DefaultHeartbeatInterval;

            this.AddMarketDataSupport();
            this.AddTransactionalSupport();
            this.RemoveSupportedMessage(MessageTypes.OrderGroupCancel);
            this.RemoveSupportedMessage(MessageTypes.OrderReplace);
            this.RemoveSupportedMessage(MessageTypes.OrderPairReplace);
            this.RemoveSupportedMessage(MessageTypes.Portfolio);

            this.AddSupportedMarketDataType(DataType.Ticks);
            this.AddSupportedMarketDataType(DataType.MarketDepth);
            this.AddSupportedMarketDataType(DataType.Level1);

            this.AddSupportedResultMessage(MessageTypes.Security);
            this.AddSupportedResultMessage(MessageTypes.SecurityLookup);
            this.AddSupportedResultMessage(MessageTypes.PortfolioLookup);
            this.AddSupportedResultMessage(MessageTypes.OrderStatus);

        }
        public override bool IsAllDownloadingSupported(DataType dataType)
         => dataType == DataType.Securities || base.IsAllDownloadingSupported(dataType);

        /// <inheritdoc />
        public override TimeSpan GetHistoryStepSize(DataType dataType, out TimeSpan iterationInterval)
        {
            var step = base.GetHistoryStepSize(dataType, out iterationInterval);

            if (dataType == DataType.Ticks)
                step = TimeSpan.FromDays(1);

            return step;
        }
        protected override bool OnSendInMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageTypes.Reset:
                    {
                        onDisconnec();
                        SendOutMessage(new ResetMessage());
                        break;
                    }

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
                        ProcessTime(message);
                        break;
                    }
                default:
                    return false;
            }
            return true;
        }
        FTAPI_Qot client;
        MarketData _market;
        MarketData market { 
            get {
                if (_market == null) {
                    _market = new MarketData(OpendIP, OpendPort, (int)StockMarket);
                }
                return _market;
            }
            set {
                _market = value;
            }
        }
       
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
            conn.Parent = this;
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
            SendOutDisconnectMessage(true);
        }


        private void Conn_Connected(bool iscoon, string errMsg)
        {
            if (iscoon)
            {
                if (market == null)
                {
                    market = new MarketData(OpendIP, OpendPort,(int)StockMarket);
                    market.Error += On_Error;
                    SubscribeMarketInfo();
                }
                if (transaction == null)
                {
                    transaction = new Transaction(OpendIP, OpendPort, Login, Password, (int)StockMarket);
                    transaction.Error += On_Error;
                    SubscribeTransactionInfo();
                }
                SendOutMessage(new ConnectMessage());
            }
            else
                SendOutError(errMsg);
        }

        private void On_Error(Exception obj)
        {
            SendOutError(obj.Message);
        }
       
    }
}
