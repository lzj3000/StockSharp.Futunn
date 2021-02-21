using System;
using Futu.OpenApi;
using Futu.OpenApi.Pb;

namespace StockSharp.Futunn.Native
{
    public class Transaction : FutuAPI
    {
        public Transaction(string ip, ushort port, string userid, string password)
        {
            OpendIP = ip;
            OpendPort = port;
            UserID = Convert.ToUInt64(userid);
            Password = CalcMD5(password);
            bool ret = InitConnectTrdSync();
            if (!ret)
                OnError("fail to connect opend");
        }

        public TrdGetAccList.Response GetAccList()
        {
            return this.GetAccListSync(Convert.ToUInt64(UserID));
        }

        public void OrderRegister(string code,double qty,double price,int trdSide,int isReal=0)
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
                      .SetTrdMarket((int)TrdCommon.TrdMarket.TrdMarket_CN)
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
                    .SetSecMarket((int)TrdCommon.TrdSecMarket.TrdSecMarket_CN_SH)
                    .Build();
            TrdPlaceOrder.Response placeOrderRsp = PlaceOrderSync(c2s);

            TrdCommon.TrdFilterConditions filterConditions = TrdCommon.TrdFilterConditions.CreateBuilder()
                      .AddCodeList(code)
                      .Build();
            TrdGetOrderFillList.Response getOrderFillListRsp = GetOrderFillListSync(accID,
                    TrdCommon.TrdMarket.TrdMarket_CN,
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
    }
}
