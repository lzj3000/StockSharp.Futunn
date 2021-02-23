using Futu.OpenApi;
using Futu.OpenApi.Pb;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Futu.OpenApi.Pb.QotCommon;
using static Futu.OpenApi.Pb.QotGetSecuritySnapshot;

namespace StockSharp.Futunn.Native
{
    public class MarketData: FutuAPI
    {
        public MarketData(string ip, ushort port,int market)
        {
            OpendIP = ip;
            OpendPort = port;
            MarketId = market;
           
        }
        private void connectSync() {
            bool ret = InitConnectQotSync();
            if (!ret)
                OnError("fail to connect opend");
        }
        public List<SecurityStaticInfo> GetSecurityList(SecurityType[] securityTypes) {
            connectSync();
            List<SecurityStaticInfo> stockCodes = new List<SecurityStaticInfo>();
            foreach (SecurityType stockType in securityTypes)
            {
                QotGetStaticInfo.C2S c2s = QotGetStaticInfo.C2S.CreateBuilder()
                        .SetMarket(MarketId)
                        .SetSecType((int)stockType)
                        .Build();
                QotGetStaticInfo.Response rsp = GetStaticInfoSync(c2s);
                if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
                {
                    OnError(string.Format("getStaticInfoSync fail: {0}\n", rsp.RetMsg));
                    return stockCodes;
                }
                foreach (SecurityStaticInfo info in rsp.S2C.StaticInfoListList)
                {
                    stockCodes.Add(info);
                }
            }

            return stockCodes;
        }
       
        public List<Snapshot> GetSnapshots(Security[] securities) {
            connectSync();
            List<Snapshot> snapshots = new List<Snapshot>();
            Response rsp = GetSecuritySnapshotSync(securities);
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                OnError(string.Format("getSecuritySnapshotSync err: retType={0} msg={1}\n", rsp.RetType, rsp.RetMsg));
            }
            else
            {
                foreach (Snapshot snapshot in rsp.S2C.SnapshotListList)
                {
                    snapshots.Add(snapshot);
                }
            }
            return snapshots;
        }
        public void GetBasicQot() { 
        
           
        }
        public void SubAllInfo(string[] securities,bool isSub)
        {
            connectSync();
            List<Security> secArr = new List<Security>();
            foreach (var code in securities) {
                secArr.Add(MakeSec(((QotMarket)MarketId), code));
            }
            List<SubType> subTypes = new List<SubType>() {
                    SubType.SubType_Basic,
                    SubType.SubType_OrderBook,
                    SubType.SubType_Ticker,
                    SubType.SubType_RT,
                    SubType.SubType_KL_Day,
            };
            QotSub.Response subRsp = SubSync(secArr, subTypes, isSub, true);
            if (subRsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                OnError(string.Format("subSync err; retType={0} msg={1}\n", subRsp.RetType, subRsp.RetMsg));
            }
        }
        public QotGetOrderBook.Response GetOrderBook(string securityCode)
        {
            connectSync();
            return GetOrderBookSync(MakeSec(QotMarket.QotMarket_CNSH_Security, securityCode), 1);
        }
        public event Action<QotUpdateBasicQot.Response> BasicQotCallback;
        public event Action<QotUpdateOrderBook.Response> OrderBookCallback;
        public event Action<QotUpdateKL.Response> KLCallback;
        public event Action<QotUpdateRT.Response> RTCallback;
        public event Action<QotUpdateTicker.Response> TickerCallback;
        public event Action<QotUpdateBroker.Response> BrokerCallback;
        /// <summary>
        /// 实时报价回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nSerialNo"></param>
        /// <param name="rsp"></param>
        public override void OnReply_UpdateBasicQot(FTAPI_Conn client, uint nSerialNo, QotUpdateBasicQot.Response rsp)
        {
            BasicQotCallback?.Invoke(rsp);
        }
        /// <summary>
        /// 实时摆盘回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nSerialNo"></param>
        /// <param name="rsp"></param>
        public override void OnReply_UpdateOrderBook(FTAPI_Conn client, uint nSerialNo, QotUpdateOrderBook.Response rsp)
        {
            OrderBookCallback?.Invoke(rsp);
        }
        /// <summary>
        /// 实时K线回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nSerialNo"></param>
        /// <param name="rsp"></param>
        public override void OnReply_UpdateKL(FTAPI_Conn client, uint nSerialNo, QotUpdateKL.Response rsp)
        {
            KLCallback?.Invoke(rsp);
        }
        /// <summary>
        /// 实时分时回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nSerialNo"></param>
        /// <param name="rsp"></param>
        public override void OnReply_UpdateRT(FTAPI_Conn client, uint nSerialNo, QotUpdateRT.Response rsp)
        {
            RTCallback?.Invoke(rsp);
        }
        /// <summary>
        /// 实时逐笔回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nSerialNo"></param>
        /// <param name="rsp"></param>
        public override void OnReply_UpdateTicker(FTAPI_Conn client, uint nSerialNo, QotUpdateTicker.Response rsp)
        {
            TickerCallback?.Invoke(rsp);
        }
        /// <summary>
        /// 实时经纪队列回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nSerialNo"></param>
        /// <param name="rsp"></param>
        public override void OnReply_UpdateBroker(FTAPI_Conn client, uint nSerialNo, QotUpdateBroker.Response rsp)
        {
            BrokerCallback?.Invoke(rsp);
        }
    }
}
