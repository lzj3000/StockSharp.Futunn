using Futu.OpenApi;
using Futu.OpenApi.Pb;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace StockSharp.Futunn.Native.Callback
{
    public class TrdCallback : FTSPI_Trd
    {
        private ulong accID;

        public void OnReply_GetAccList(FTAPI_Conn client, uint nSerialNo, TrdGetAccList.Response rsp)
        {
            Console.WriteLine("Recv GetAccList: {0} {1}", nSerialNo, rsp);
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                Console.WriteLine("error code is {0}", rsp.RetMsg);
            }
            else
            {
                this.accID = rsp.S2C.AccListList[0].AccID;
                FTAPI_Trd trd = client as FTAPI_Trd;
                MD5 md5 = MD5.Create();
                byte[] encryptionBytes = md5.ComputeHash(Encoding.UTF8.GetBytes("123123"));
                string unlockPwdMd5 = BitConverter.ToString(encryptionBytes).Replace("-", "").ToLower();
                TrdUnlockTrade.Request req = TrdUnlockTrade.Request.CreateBuilder().SetC2S(TrdUnlockTrade.C2S.CreateBuilder().SetUnlock(true).SetPwdMD5(unlockPwdMd5)).Build();
                uint serialNo = trd.UnlockTrade(req);
                Console.WriteLine("Send UnlockTrade: {0}", serialNo);

            }
        }

        public void OnReply_UnlockTrade(FTAPI_Conn client, uint nSerialNo, TrdUnlockTrade.Response rsp)
        {
            Console.WriteLine("Recv UnlockTrade: {0} {1}", nSerialNo, rsp);
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                Console.WriteLine("error code is {0}", rsp.RetMsg);
            }
            else
            {
                FTAPI_Trd trd = client as FTAPI_Trd;

                TrdPlaceOrder.Request.Builder req = TrdPlaceOrder.Request.CreateBuilder();
                TrdPlaceOrder.C2S.Builder cs = TrdPlaceOrder.C2S.CreateBuilder();
                Common.PacketID.Builder packetID = Common.PacketID.CreateBuilder().SetConnID(trd.GetConnectID()).SetSerialNo(0);
                TrdCommon.TrdHeader.Builder trdHeader = TrdCommon.TrdHeader.CreateBuilder().SetAccID(this.accID).SetTrdEnv((int)TrdCommon.TrdEnv.TrdEnv_Real).SetTrdMarket((int)TrdCommon.TrdMarket.TrdMarket_HK);
                cs.SetPacketID(packetID).SetHeader(trdHeader).SetTrdSide((int)TrdCommon.TrdSide.TrdSide_Sell).SetOrderType((int)TrdCommon.OrderType.OrderType_AbsoluteLimit).SetCode("01810").SetQty(100.00).SetPrice(10.2).SetAdjustPrice(true);
                req.SetC2S(cs);

                uint serialNo = trd.PlaceOrder(req.Build());
                Console.WriteLine("Send PlaceOrder: {0}, {1}", serialNo, req);
            }

        }

        public void OnReply_SubAccPush(FTAPI_Conn client, uint nSerialNo, TrdSubAccPush.Response rsp)
        {

        }

        public void OnReply_GetFunds(FTAPI_Conn client, uint nSerialNo, TrdGetFunds.Response rsp)
        {

        }

        public void OnReply_GetPositionList(FTAPI_Conn client, uint nSerialNo, TrdGetPositionList.Response rsp)
        {

        }

        public void OnReply_GetMaxTrdQtys(FTAPI_Conn client, uint nSerialNo, TrdGetMaxTrdQtys.Response rsp)
        {

        }

        public void OnReply_GetOrderList(FTAPI_Conn client, uint nSerialNo, TrdGetOrderList.Response rsp)
        {

        }

        public void OnReply_GetOrderFillList(FTAPI_Conn client, uint nSerialNo, TrdGetOrderFillList.Response rsp)
        {

        }

        public void OnReply_GetHistoryOrderList(FTAPI_Conn client, uint nSerialNo, TrdGetHistoryOrderList.Response rsp)
        {

        }

        public void OnReply_GetHistoryOrderFillList(FTAPI_Conn client, uint nSerialNo, TrdGetHistoryOrderFillList.Response rsp)
        {

        }

        public void OnReply_UpdateOrder(FTAPI_Conn client, uint nSerialNo, TrdUpdateOrder.Response rsp)
        {
            Console.WriteLine("Recv UpdateOrder: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_UpdateOrderFill(FTAPI_Conn client, uint nSerialNo, TrdUpdateOrderFill.Response rsp)
        {
            Console.WriteLine("Recv UpdateOrderFill: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_PlaceOrder(FTAPI_Conn client, uint nSerialNo, TrdPlaceOrder.Response rsp)
        {
            Console.WriteLine("Recv PlaceOrder: {0} {1}", nSerialNo, rsp);
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                Console.WriteLine("error code is {0}", rsp.RetMsg);
            }
        }

        public void OnReply_ModifyOrder(FTAPI_Conn client, uint nSerialNo, TrdModifyOrder.Response rsp)
        {
        }
    }
}
