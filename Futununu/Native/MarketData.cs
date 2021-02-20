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
        public MarketData(string ip, ushort port) {
            OpendIP = ip;
            OpendPort = port;
            bool ret = InitConnectQotSync();
            if (!ret)
                OnError("fail to connect opend");
        }

        public List<SecurityStaticInfo> GetSecurityList(SecurityType[] securityTypes) {
            var market = QotMarket.QotMarket_CNSH_Security;
            List<SecurityStaticInfo> stockCodes = new List<SecurityStaticInfo>();
            foreach (SecurityType stockType in securityTypes)
            {
                QotGetStaticInfo.C2S c2s = QotGetStaticInfo.C2S.CreateBuilder()
                        .SetMarket((int)market)
                        .SetSecType((int)stockType)
                        .Build();
                QotGetStaticInfo.Response rsp = GetStaticInfoSync(c2s);
                if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
                {
                    Console.Error.Write("getStaticInfoSync fail: {0}\n", rsp.RetMsg);
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

            List<Snapshot> snapshots = new List<Snapshot>();
            Response rsp = GetSecuritySnapshotSync(securities);
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                Console.Error.Write("getSecuritySnapshotSync err: retType={0} msg={1}\n", rsp.RetType, rsp.RetMsg);
            }
            else
            {
                foreach (QotGetSecuritySnapshot.Snapshot snapshot in rsp.S2C.SnapshotListList)
                {
                    snapshots.Add(snapshot);

                }
            }
            return snapshots;
        }

        public void SubSecurityAllInfo(string[] securities)
        {
            List<Security> secArr = new List<Security>();
            foreach (var code in securities) {
                secArr.Add(MakeSec(QotMarket.QotMarket_CNSH_Security, code));
            }
            List<SubType> subTypes = new List<SubType>() {
                    SubType.SubType_Basic,
                    SubType.SubType_Broker,
                    SubType.SubType_OrderBook,
                    SubType.SubType_Ticker,
                    SubType.SubType_RT,
                    SubType.SubType_KL_Day,
            };
            QotSub.Response subRsp = SubSync(secArr, subTypes, true, true);
            if (subRsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                Console.Error.Write("subSync err; retType={0} msg={1}\n", subRsp.RetType, subRsp.RetMsg);
            }
        }
        public void SubscribeOrderBook(string securityCode) {
          
        }
        public override void OnReply_UpdateBasicQot(FTAPI_Conn client, uint nSerialNo, QotUpdateBasicQot.Response rsp)
        {
           
        }
        public override void OnReply_UpdateBroker(FTAPI_Conn client, uint nSerialNo, QotUpdateBroker.Response rsp)
        {
           
        }
        public override void OnReply_UpdateOrderBook(FTAPI_Conn client, uint nSerialNo, QotUpdateOrderBook.Response rsp)
        {
            
        }

    }
}
