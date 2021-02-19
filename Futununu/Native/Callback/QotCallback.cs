using Futu.OpenApi;
using Futu.OpenApi.Pb;
using Google.ProtocolBuffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockSharp.Futunn.Native.Callback
{
    public class QotCallback : FTSPI_Qot
    {
        public void OnReply_GetGlobalState(FTAPI_Conn client, uint nSerialNo, GetGlobalState.Response rsp)
        {
            Console.WriteLine("OnReply_GetGlobalState: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_Sub(FTAPI_Conn client, uint nSerialNo, QotSub.Response rsp)
        {

        }

        public void OnReply_RegQotPush(FTAPI_Conn client, uint nSerialNo, QotRegQotPush.Response rsp)
        {

        }

        public void OnReply_GetSubInfo(FTAPI_Conn client, uint nSerialNo, QotGetSubInfo.Response rsp)
        {

        }

        public void OnReply_GetTicker(FTAPI_Conn client, uint nSerialNo, QotGetTicker.Response rsp)
        {

        }

        public void OnReply_GetBasicQot(FTAPI_Conn client, uint nSerialNo, QotGetBasicQot.Response rsp)
        {

        }

        public void OnReply_GetOrderBook(FTAPI_Conn client, uint nSerialNo, QotGetOrderBook.Response rsp)
        {

        }

        public void OnReply_GetKL(FTAPI_Conn client, uint nSerialNo, QotGetKL.Response rsp)
        {

        }

        public void OnReply_GetRT(FTAPI_Conn client, uint nSerialNo, QotGetRT.Response rsp)
        {

        }

        public void OnReply_GetBroker(FTAPI_Conn client, uint nSerialNo, QotGetBroker.Response rsp)
        {

        }

        public void OnReply_GetHistoryKL(FTAPI_Conn client, uint nSerialNo, QotGetHistoryKL.Response rsp)
        {

        }

        public void OnReply_GetHistoryKLPoints(FTAPI_Conn client, uint nSerialNo, QotGetHistoryKLPoints.Response rsp)
        {

        }

        public void OnReply_GetRehab(FTAPI_Conn client, uint nSerialNo, QotGetRehab.Response rsp)
        {

        }

        public void OnReply_RequestRehab(FTAPI_Conn client, uint nSerialNo, QotRequestRehab.Response rsp)
        {

        }

        public void OnReply_RequestHistoryKL(FTAPI_Conn client, uint nSerialNo, QotRequestHistoryKL.Response rsp)
        {

        }

        public void OnReply_RequestHistoryKLQuota(FTAPI_Conn client, uint nSerialNo, QotRequestHistoryKLQuota.Response rsp)
        {

        }

        public void OnReply_GetTradeDate(FTAPI_Conn client, uint nSerialNo, QotGetTradeDate.Response rsp)
        {

        }

        public void OnReply_GetStaticInfo(FTAPI_Conn client, uint nSerialNo, QotGetStaticInfo.Response rsp)
        {

        }

        public void OnReply_GetSecuritySnapshot(FTAPI_Conn client, uint nSerialNo, QotGetSecuritySnapshot.Response rsp)
        {

        }

        public void OnReply_GetPlateSet(FTAPI_Conn client, uint nSerialNo, QotGetPlateSet.Response rsp)
        {

        }

        public void OnReply_GetPlateSecurity(FTAPI_Conn client, uint nSerialNo, QotGetPlateSecurity.Response rsp)
        {

        }

        public void OnReply_GetReference(FTAPI_Conn client, uint nSerialNo, QotGetReference.Response rsp)
        {

        }

        public void OnReply_GetOwnerPlate(FTAPI_Conn client, uint nSerialNo, QotGetOwnerPlate.Response rsp)
        {

        }

        public void OnReply_GetHoldingChangeList(FTAPI_Conn client, uint nSerialNo, QotGetHoldingChangeList.Response rsp)
        {

        }

        public void OnReply_GetOptionChain(FTAPI_Conn client, uint nSerialNo, QotGetOptionChain.Response rsp)
        {

        }

        public void OnReply_GetWarrant(FTAPI_Conn client, uint nSerialNo, QotGetWarrant.Response rsp)
        {

        }

        public void OnReply_GetCapitalFlow(FTAPI_Conn client, uint nSerialNo, QotGetCapitalFlow.Response rsp)
        {

        }

        public void OnReply_GetCapitalDistribution(FTAPI_Conn client, uint nSerialNo, QotGetCapitalDistribution.Response rsp)
        {

        }

        public void OnReply_GetUserSecurity(FTAPI_Conn client, uint nSerialNo, QotGetUserSecurity.Response rsp)
        {
            Console.WriteLine(rsp.S2C.ToJson());
        }

        public void OnReply_SetPriceReminder(FTAPI_Conn client, uint nSerialNo, QotSetPriceReminder.Response rsp)
        {
            Console.WriteLine("OnReply_SetPriceReminder: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_GetPriceReminder(FTAPI_Conn client, uint nSerialNo, QotGetPriceReminder.Response rsp)
        {
            Console.WriteLine("OnReply_GetPriceReminder: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_ModifyUserSecurity(FTAPI_Conn client, uint nSerialNo, QotModifyUserSecurity.Response rsp)
        {

        }

        public void OnReply_Notify(FTAPI_Conn client, uint nSerialNo, Notify.Response rsp)
        {

        }


        public void OnReply_UpdateBasicQot(FTAPI_Conn client, uint nSerialNo, QotUpdateBasicQot.Response rsp)
        {

        }

        public void OnReply_UpdateKL(FTAPI_Conn client, uint nSerialNo, QotUpdateKL.Response rsp)
        {

        }

        public void OnReply_UpdateRT(FTAPI_Conn client, uint nSerialNo, QotUpdateRT.Response rsp)
        {

        }

        public void OnReply_UpdateTicker(FTAPI_Conn client, uint nSerialNo, QotUpdateTicker.Response rsp)
        {
            Console.WriteLine("OnReply_UpdateTicker: {0} {1}", nSerialNo, rsp.S2C.ToJson());
        }

        public void OnReply_UpdateOrderBook(FTAPI_Conn client, uint nSerialNo, QotUpdateOrderBook.Response rsp)
        {

        }

        public void OnReply_UpdateBroker(FTAPI_Conn client, uint nSerialNo, QotUpdateBroker.Response rsp)
        {

        }

        public void OnReply_UpdateOrderDetail(FTAPI_Conn client, uint nSerialNo, QotUpdateOrderDetail.Response rsp)
        {

        }

        public void OnReply_UpdatePriceReminder(FTAPI_Conn client, uint nSerialNo, QotUpdatePriceReminder.Response rsp)
        {
            Console.WriteLine("OnReply_UpdatePriceReminder: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_StockFilter(FTAPI_Conn client, uint nSerialNo, QotStockFilter.Response rsp)
        {
            Console.WriteLine("OnReply_StockFilter: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_GetCodeChange(FTAPI_Conn client, uint nSerialNo, QotGetCodeChange.Response rsp)
        {

        }


        public void OnReply_GetIpoList(FTAPI_Conn client, uint nSerialNo, QotGetIpoList.Response rsp)
        {
            throw new NotImplementedException();
        }

        public void OnReply_GetFutureInfo(FTAPI_Conn client, uint nSerialNo, QotGetFutureInfo.Response rsp)
        {
            throw new NotImplementedException();
        }

        public void OnReply_RequestTradeDate(FTAPI_Conn client, uint nSerialNo, QotRequestTradeDate.Response rsp)
        {
            throw new NotImplementedException();
        }


        public void OnReply_GetUserSecurityGroup(FTAPI_Conn client, uint nSerialNo, QotGetUserSecurityGroup.Response rsp)
        {
            Console.WriteLine("OnReply_GetUserSecurityGroup: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_GetMarketState(FTAPI_Conn client, uint nSerialNo, QotGetMarketState.Response rsp)
        {
            Console.WriteLine("OnReply_GetMarketState: {0} {1}", nSerialNo, rsp);
        }
    }
}
