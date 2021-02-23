using System;
using System.Threading;
using Futu.OpenApi;
using Futu.OpenApi.Pb;

namespace StockSharp.Futunn.Native
{
    public class Transaction : FutuAPI
    {
        public Transaction(string ip, ushort port, string userid, string password, int market)
        {
            OpendIP = ip;
            OpendPort = port;
            UserID = Convert.ToUInt64(userid);
            Password = CalcMD5(password);
            MarketId = market;
            bool ret = InitConnectTrdSync();
            if (!ret)
                OnError("fail to connect opend");
        }
        protected int GetTrdMarket() {
            if (MarketId == 1)
                return 1;
            if (MarketId == 11)
                return 2;
            if (MarketId == 21||MarketId==22)
                return 3;
            return 0;
        }
        protected int GetTrdSecMarket() {
            if (MarketId == 1)
                return 1;
            if (MarketId == 11)
                return 2;
            if (MarketId == 21 || MarketId == 22)
                return MarketId + 10;
            return 0;
        }
        public TrdGetAccList.Response GetAccList()
        {
            return this.GetAccListSync(Convert.ToUInt64(UserID));
        }

        public void OrderRegister(string code, double qty, double price, int trdSide, int isReal = 0)
        {
            TrdUnlockTrade.Response unlockTradeRsp = UnlockTradeSync(Password, true);
            if (unlockTradeRsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                OnError(string.Format("unlockTradeSync err; retType={0} msg={1}\n", unlockTradeRsp.RetType, unlockTradeRsp.RetMsg));
            }

            TrdGetAccList.Response accList = GetAccList();
            var accID = accList.S2C.GetAccList(0).AccID;

            TrdCommon.TrdHeader header = TrdCommon.TrdHeader.CreateBuilder()
                      .SetTrdEnv(isReal)
                      .SetAccID(accID)
                      .SetTrdMarket(GetTrdMarket())
                      .Build();
            TrdPlaceOrder.C2S c2s = TrdPlaceOrder.C2S.CreateBuilder()
                    .SetPacketID(trd.NextPacketID())
                    .SetHeader(header)
                    .SetTrdSide(trdSide)
                    .SetOrderType((int)TrdCommon.OrderType.OrderType_Normal)
                    .SetCode(code)
                    .SetQty(qty)
                    .SetPrice(price)
                    .SetAdjustPrice(true)
                    .SetSecMarket(GetTrdSecMarket())
                    .Build();
            TrdPlaceOrder.Response placeOrderRsp = PlaceOrderSync(c2s);

            TrdCommon.TrdFilterConditions filterConditions = TrdCommon.TrdFilterConditions.CreateBuilder()
                      .AddCodeList(code)
                      .Build();
            TrdGetOrderFillList.Response getOrderFillListRsp = GetOrderFillListSync(accID,
                    ((TrdCommon.TrdMarket)GetTrdMarket()),
                    (TrdCommon.TrdEnv)isReal, false, filterConditions);
        }
        public event Action<TrdUpdateOrder.Response> OrderStates;
        public event Action<TrdUpdateOrderFill.Response> OrderTradeFill;
        /// <summary>
        /// 订单状态回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nSerialNo"></param>
        /// <param name="rsp"></param>
        public override void OnReply_UpdateOrder(FTAPI_Conn client, uint nSerialNo, TrdUpdateOrder.Response rsp)
        {
            OrderStates?.Invoke(rsp);
        }

        /// <summary>
        /// 订单成交回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nSerialNo"></param>
        /// <param name="rsp"></param>
        public override void OnReply_UpdateOrderFill(FTAPI_Conn client, uint nSerialNo, TrdUpdateOrderFill.Response rsp)
        {
            OrderTradeFill?.Invoke(rsp);
        }
        /// <summary>
        /// 撤销订单（同步）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        public TrdModifyOrder.Request OrderCancelSync(ulong orderId)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    TrdCommon.TrdHeader header = TrdCommon.TrdHeader.CreateBuilder()
             .SetAccID(281756457888247915L)
             .SetTrdEnv((int)TrdCommon.TrdEnv.TrdEnv_Simulate)
             .SetTrdMarket(GetTrdMarket())
             .Build();
                    TrdModifyOrder.C2S c2s = TrdModifyOrder.C2S.CreateBuilder()
                            .SetPacketID(trd.NextPacketID())
                            .SetHeader(header)
                            .SetOrderID(orderId)
                            .SetModifyOrderOp((int)TrdCommon.ModifyOrderOp.ModifyOrderOp_Cancel)
                        .Build();
                    TrdModifyOrder.Request req = TrdModifyOrder.Request.CreateBuilder().SetC2S(c2s).Build();
                    var sn = trd.ModifyOrder(req);
                    reqInfo = new ReqInfo(ProtoID.TrdModifyOrder, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdModifyOrder.Request)reqInfo.Rsp;
            }
        }
    }
}
