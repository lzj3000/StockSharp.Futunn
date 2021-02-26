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
            if (this.qotConnStatus != ConnStatus.READY)
            {
                bool ret = InitConnectQotSync();
                if (!ret)
                    OnError("fail to connect opend");
            }
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
        public IList<Snapshot> GetSnapshots(string[] codes)
        {
            connectSync();
            List<Security> list = new List<Security>();
            foreach (var code in codes)
                list.Add(MakeSec(((QotMarket)MarketId), code));
            List<Snapshot> snapshots = new List<Snapshot>();
            Response rsp = GetSecuritySnapshotSync(list.ToArray());
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                OnError(string.Format("getSecuritySnapshotSync err: retType={0} msg={1}\n", rsp.RetType, rsp.RetMsg));
            }
            return rsp.S2C.SnapshotListList;
        }
        public Snapshot GetSnapshots(string code)
        {
            connectSync();
            Security security = MakeSec(((QotMarket)MarketId), code);
            List<Snapshot> snapshots = new List<Snapshot>();
            Response rsp = GetSecuritySnapshotSync(new Security[] { security });
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                OnError(string.Format("getSecuritySnapshotSync err: retType={0} msg={1}\n", rsp.RetType, rsp.RetMsg));
            }
            return rsp.S2C.SnapshotListList[0];
        }
        public QotGetBasicQot.Request GetBasicQot(string code)
        {
            connectSync();
            List<Security> secArr = new List<Security>();
            secArr.Add(MakeSec(((QotMarket)MarketId), code));
            List<SubType> subTypes = new List<SubType>() {
                    SubType.SubType_Basic
            };
            QotSub.Response subRsp = SubSync(secArr, subTypes, true, false);
            if (subRsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                OnError(string.Format("subSync err; retType={0} msg={1}\n", subRsp.RetType, subRsp.RetMsg));
            }
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();
            lock (syncEvent)
            {
                lock (trdLock)
                {
                    QotCommon.Security sec = QotCommon.Security.CreateBuilder()
                   .SetMarket(MarketId)
                   .SetCode(code)
                   .Build();
                    QotGetBasicQot.C2S c2s = QotGetBasicQot.C2S.CreateBuilder()
                            .AddSecurityList(sec)
                            .Build();
                    QotGetBasicQot.Request req = QotGetBasicQot.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint seqNo = qot.GetBasicQot(req);
                    reqInfo = new ReqInfo(ProtoID.TrdModifyOrder, syncEvent);
                    trdReqInfoMap.Add(seqNo, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (QotGetBasicQot.Request)reqInfo.Rsp;
            }
        }
        public QotGetTicker.Request GetTicker(string code)
        {
            connectSync();
            List<Security> secArr = new List<Security>();
            secArr.Add(MakeSec(((QotMarket)MarketId), code));
            List<SubType> subTypes = new List<SubType>() {
                    SubType.SubType_Ticker
            };
            QotSub.Response subRsp = SubSync(secArr, subTypes, true, false);
            if (subRsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                OnError(string.Format("subSync err; retType={0} msg={1}\n", subRsp.RetType, subRsp.RetMsg));
            }
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();
            lock (syncEvent)
            {
                lock (trdLock)
                {
                    QotCommon.Security sec = QotCommon.Security.CreateBuilder()
                    .SetMarket(MarketId)
                    .SetCode(code)
                    .Build();
                    QotGetTicker.C2S c2s = QotGetTicker.C2S.CreateBuilder()
                            .SetSecurity(sec)
                            .SetMaxRetNum(10)
                            .Build();
                    QotGetTicker.Request req = QotGetTicker.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint seqNo = qot.GetTicker(req);
                    reqInfo = new ReqInfo(ProtoID.TrdModifyOrder, syncEvent);
                    trdReqInfoMap.Add(seqNo, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (QotGetTicker.Request)reqInfo.Rsp;
            }    
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
