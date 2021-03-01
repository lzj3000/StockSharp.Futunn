using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using Futu.OpenApi;
using Futu.OpenApi.Pb;
using Google.ProtocolBuffers;
using System.Security.Cryptography;
using StockSharp.Logging;

namespace StockSharp.Futunn.Native
{

    public enum ConnStatus
    {
        DISCONNECT,
        READY
    }

   public class ReqInfo
    {
        internal ProtoID ProtoID { get; private set; }
        internal object SyncEvent { get; private set; }
        internal object Rsp { get; set; }

        internal ReqInfo(ProtoID protoID, object syncEvent)
        {
            this.ProtoID = protoID;
            this.SyncEvent = syncEvent;
        }
    }
    public class FutuAPI :FTSPI_Conn, FTSPI_Qot, FTSPI_Trd
    {
        protected object qotLock = new object();
        protected object trdLock = new object();
        protected FTAPI_Qot qot = new FTAPI_Qot();
        protected FTAPI_Trd trd = new FTAPI_Trd();
        protected ConnStatus qotConnStatus = ConnStatus.DISCONNECT;
        protected ConnStatus trdConnStatus = ConnStatus.DISCONNECT;
        protected Dictionary<uint, ReqInfo> qotReqInfoMap = new Dictionary<uint, ReqInfo>();
        protected Dictionary<uint, ReqInfo> trdReqInfoMap = new Dictionary<uint, ReqInfo>();

        public event Action<Exception> Error;

        protected void OnError(string msg) {
            Error?.Invoke(new Exception(msg));
        }
        protected void OnError(Exception err)
        {
            Error?.Invoke(err);
        }
        public FutuAPI(string ip, ushort port, string user, string pass) {
            OpendIP = ip;
            OpendPort = port;
            UserID = Convert.ToUInt64(user);
            Password = CalcMD5(pass);
        }
        public FutuAPI() { }
        public string OpendIP { get; protected set; }
        public ushort OpendPort { get; protected set; }
        public int MarketId { get; protected set; }

        public ulong UserID { get; protected set; }
        public string Password { get; protected set; }
        protected string CalcMD5(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] encryptionBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            string result = BitConverter.ToString(encryptionBytes).Replace("-", "").ToLower();
            return result;
        }
        public void OnInitConnect(FTAPI_Conn client, long errCode, string desc)
        {
            if (client is FTAPI_Qot)
            {
                lock (qotLock)
                {
                    if (errCode == 0)
                    {
                        qotConnStatus = ConnStatus.READY;
                        Monitor.PulseAll(qotLock);
                    }
                    else
                    {
                        OnError(desc);
                    }
                }
            }

            if (client is FTAPI_Trd)
            {
                lock (trdLock)
                {
                    if (errCode == 0)
                    {
                        trdConnStatus = ConnStatus.READY;
                        Monitor.PulseAll(trdLock);
                    }
                    else
                    {
                        OnError(desc);
                    }

                }
            }
        }

        public void OnDisconnect(FTAPI_Conn client, long errCode)
        {
            if (client is FTAPI_Qot)
            {
                lock (qotLock)
                {
                    qotConnStatus = ConnStatus.DISCONNECT;
                    return;
                }
            }

            if (client is FTAPI_Trd)
            {
                lock (trdLock)
                {
                    trdConnStatus = ConnStatus.DISCONNECT;
                }
            }
        }

        public bool InitConnectQotSync()
        {
            qot.SetConnCallback(this);
            qot.SetQotCallback(this);
            lock (qotLock)
            {
                bool ret = qot.InitConnect(OpendIP, OpendPort, false);
                if (!ret)
                    return false;
                Monitor.Wait(qotLock);
                return qotConnStatus == ConnStatus.READY;
            }
        }

        public bool InitConnectTrdSync()
        {
            trd.SetConnCallback(this);
            trd.SetTrdCallback(this);
            lock (trdLock)
            {
                bool ret = trd.InitConnect(OpendIP, OpendPort, false);
                if (!ret)
                    return false;
                Monitor.Wait(trdLock);
                return trdConnStatus == ConnStatus.READY;
            }
        }

        public QotGetSecuritySnapshot.Response GetSecuritySnapshotSync(ICollection<QotCommon.Security> secList)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (qotLock)
                {
                    if (qotConnStatus != ConnStatus.READY)
                        return null;
                    QotGetSecuritySnapshot.C2S c2s = QotGetSecuritySnapshot.C2S.CreateBuilder()
                            .AddRangeSecurityList(secList)
                            .Build();
                    QotGetSecuritySnapshot.Request req = QotGetSecuritySnapshot.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = qot.GetSecuritySnapshot(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.QotGetSecuritySnapshot, syncEvent);
                    qotReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (QotGetSecuritySnapshot.Response)reqInfo.Rsp;
            }
        }

        public QotGetStaticInfo.Response GetStaticInfoSync(QotGetStaticInfo.C2S c2s)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (qotLock)
                {
                    if (qotConnStatus != ConnStatus.READY)
                        return null;
                    QotGetStaticInfo.Request req = QotGetStaticInfo.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = qot.GetStaticInfo(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.QotGetStaticInfo, syncEvent);
                    qotReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (QotGetStaticInfo.Response)reqInfo.Rsp;
            }
        }

        public QotSub.Response SubSync(List<QotCommon.Security> secList,
                                List<QotCommon.SubType> subTypeList,
                                bool isSub,
                                bool isRegPush)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (qotLock)
                {
                    if (qotConnStatus != ConnStatus.READY)
                        return null;
                    QotSub.C2S c2s = QotSub.C2S.CreateBuilder()
                            .AddRangeSecurityList(secList)
                            .AddRangeSubTypeList(subTypeList.Select(t => (int)t).ToList())
                            .SetIsSubOrUnSub(isSub)
                            .SetIsRegOrUnRegPush(isRegPush)
                            .Build();
                    QotSub.Request req = QotSub.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = qot.Sub(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.QotSub, syncEvent);
                    qotReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (QotSub.Response)reqInfo.Rsp;
            }
        }

        public QotGetOrderBook.Response GetOrderBookSync(QotCommon.Security sec, int num)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (qotLock)
                {
                    if (qotConnStatus != ConnStatus.READY)
                        return null;
                    QotGetOrderBook.C2S c2s = QotGetOrderBook.C2S.CreateBuilder()
                            .SetSecurity(sec)
                            .SetNum(num)
                            .Build();
                    QotGetOrderBook.Request req = QotGetOrderBook.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = qot.GetOrderBook(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.QotGetOrderBook, syncEvent);
                    qotReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (QotGetOrderBook.Response)reqInfo.Rsp;
            }
        }

        public QotRequestHistoryKL.Response RequestHistoryKLSync(QotCommon.Security sec,
                                                          QotCommon.KLType klType,
                                                          QotCommon.RehabType rehabType,
                                                          String beginTime,
                                                          String endTime,
                                                          int? count,
                                                          long? klFields,
                                                          byte[] nextReqKey,
                                                          bool extendedTime)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (qotLock)
                {
                    if (qotConnStatus != ConnStatus.READY)
                        return null;
                    QotRequestHistoryKL.C2S.Builder c2s = QotRequestHistoryKL.C2S.CreateBuilder()
                            .SetSecurity(sec)
                            .SetKlType((int)klType)
                            .SetRehabType((int)rehabType)
                            .SetBeginTime(beginTime)
                            .SetEndTime(endTime)
                            .SetExtendedTime(extendedTime);
                    if (count.HasValue)
                    {
                        c2s.SetMaxAckKLNum(count.Value);
                    }
                    if (klFields.HasValue)
                    {
                        c2s.SetNeedKLFieldsFlag(klFields.Value);
                    }
                    if (nextReqKey.Length > 0)
                    {
                        c2s.SetNextReqKey(ByteString.CopyFrom(nextReqKey));
                    }
                    QotRequestHistoryKL.Request req = QotRequestHistoryKL.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = qot.RequestHistoryKL(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.QotRequestHistoryKL, syncEvent);
                    qotReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (QotRequestHistoryKL.Response)reqInfo.Rsp;
            }

        }

        public TrdPlaceOrder.Response PlaceOrderSync(TrdPlaceOrder.C2S c2s)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdPlaceOrder.Request req = TrdPlaceOrder.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.PlaceOrder(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdPlaceOrder, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdPlaceOrder.Response)reqInfo.Rsp;
            }
        }

        public TrdUnlockTrade.Response UnlockTradeSync(String pwdMD5, bool isUnlock)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdUnlockTrade.C2S c2s = TrdUnlockTrade.C2S.CreateBuilder()
                            .SetPwdMD5(pwdMD5)
                            .SetUnlock(isUnlock)
                            .Build();
                    TrdUnlockTrade.Request req = TrdUnlockTrade.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.UnlockTrade(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdUnlockTrade, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdUnlockTrade.Response)reqInfo.Rsp;
            }
        }

        public TrdSubAccPush.Response subTrdAccPushSync(List<ulong> accList)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdSubAccPush.C2S c2s = TrdSubAccPush.C2S.CreateBuilder().AddRangeAccIDList(accList).Build();
                    TrdSubAccPush.Request req = TrdSubAccPush.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.SubAccPush(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdSubAccPush, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdSubAccPush.Response)reqInfo.Rsp;
            }
        }

        public TrdGetAccList.Response GetAccListSync(ulong userID)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdGetAccList.C2S c2s = TrdGetAccList.C2S.CreateBuilder().SetUserID(userID).Build();
                    TrdGetAccList.Request req = TrdGetAccList.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.GetAccList(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdGetAccList, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdGetAccList.Response)reqInfo.Rsp;
            }
        }

        public TrdGetFunds.Response GetFundsSync(ulong accID, TrdCommon.TrdMarket trdMarket, TrdCommon.TrdEnv trdEnv,
                                          bool isRefreshCache,
                                          TrdCommon.Currency currency)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdCommon.TrdHeader trdHeader = MakeTrdHeader(trdEnv, accID, trdMarket);
                    TrdGetFunds.C2S c2s = TrdGetFunds.C2S.CreateBuilder()
                            .SetHeader(trdHeader)
                            .SetCurrency((int)currency)
                            .SetRefreshCache(isRefreshCache)
                            .Build();
                    TrdGetFunds.Request req = TrdGetFunds.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.GetFunds(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdGetFunds, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdGetFunds.Response)reqInfo.Rsp;
            }
        }

        public TrdGetOrderList.Response GetOrderListSync(ulong accID, TrdCommon.TrdMarket trdMarket, TrdCommon.TrdEnv trdEnv,
                                              bool isRefreshCache,
                                              TrdCommon.TrdFilterConditions filterConditions,
                                              List<TrdCommon.OrderStatus> filterStatusList)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdCommon.TrdHeader trdHeader = MakeTrdHeader(trdEnv, accID, trdMarket);
                    TrdGetOrderList.C2S.Builder c2s = TrdGetOrderList.C2S.CreateBuilder()
                            .SetHeader(trdHeader)
                            .SetRefreshCache(isRefreshCache);
                    if (filterConditions != null)
                    {
                        c2s.SetFilterConditions(filterConditions);
                    }
                    if (filterStatusList.Count > 0)
                    {
                        foreach (TrdCommon.OrderStatus status in filterStatusList)
                        {
                            c2s.AddFilterStatusList((int)status);
                        }
                    }
                    TrdGetOrderList.Request req = TrdGetOrderList.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.GetOrderList(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdGetOrderList, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdGetOrderList.Response)reqInfo.Rsp;
            }
        }

        public TrdGetOrderFillList.Response GetOrderFillListSync(ulong accID, TrdCommon.TrdMarket trdMarket, TrdCommon.TrdEnv trdEnv,
                                                          bool isRefreshCache,
                                                          TrdCommon.TrdFilterConditions filterConditions)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdCommon.TrdHeader trdHeader = MakeTrdHeader(trdEnv, accID, trdMarket);
                    TrdGetOrderFillList.C2S.Builder c2s = TrdGetOrderFillList.C2S.CreateBuilder()
                            .SetHeader(trdHeader)
                            .SetRefreshCache(isRefreshCache);
                    if (filterConditions != null)
                    {
                        c2s.SetFilterConditions(filterConditions);
                    }
                    TrdGetOrderFillList.Request req = TrdGetOrderFillList.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.GetOrderFillList(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdGetOrderFillList, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdGetOrderFillList.Response)reqInfo.Rsp;
            }
        }

        public TrdGetHistoryOrderList.Response GetHistoryOrderListSync(ulong accID, TrdCommon.TrdMarket trdMarket, TrdCommon.TrdEnv trdEnv,
                                                                TrdCommon.TrdFilterConditions filterConditions,
                                                                List<TrdCommon.OrderStatus> filterStatusList)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdCommon.TrdHeader trdHeader = MakeTrdHeader(trdEnv, accID, trdMarket);
                    TrdGetHistoryOrderList.C2S.Builder c2s = TrdGetHistoryOrderList.C2S.CreateBuilder()
                            .SetHeader(trdHeader);
                    if (filterConditions != null)
                    {
                        c2s.SetFilterConditions(filterConditions);
                    }
                    if (filterStatusList.Count > 0)
                    {
                        foreach (TrdCommon.OrderStatus status in filterStatusList)
                        {
                            c2s.AddFilterStatusList((int)status);
                        }
                    }
                    TrdGetHistoryOrderList.Request req = TrdGetHistoryOrderList.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.GetHistoryOrderList(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdGetHistoryOrderList, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdGetHistoryOrderList.Response)reqInfo.Rsp;
            }
        }

        public TrdGetHistoryOrderFillList.Response GetHistoryOrderFillListSync(ulong accID, TrdCommon.TrdMarket trdMarket, TrdCommon.TrdEnv trdEnv,
                                                                        TrdCommon.TrdFilterConditions filterConditions)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdCommon.TrdHeader trdHeader = MakeTrdHeader(trdEnv, accID, trdMarket);
                    TrdGetHistoryOrderFillList.C2S.Builder c2s = TrdGetHistoryOrderFillList.C2S.CreateBuilder()
                            .SetHeader(trdHeader);
                    if (filterConditions != null)
                    {
                        c2s.SetFilterConditions(filterConditions);
                    }
                    TrdGetHistoryOrderFillList.Request req = TrdGetHistoryOrderFillList.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.GetHistoryOrderFillList(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdGetHistoryOrderFillList, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdGetHistoryOrderFillList.Response)reqInfo.Rsp;
            }
        }

        public TrdGetPositionList.Response GetPositionListSync(ulong accID, TrdCommon.TrdMarket trdMarket, TrdCommon.TrdEnv trdEnv,
                                                        TrdCommon.TrdFilterConditions filterConditions,
                                                        double? filterPLRatioMin,
                                                        double? filterPLRatioMax,
                                                        bool isRefreshCache)
        {
            ReqInfo reqInfo = null;
            Object syncEvent = new Object();

            lock (syncEvent)
            {
                lock (trdLock)
                {
                    if (trdConnStatus != ConnStatus.READY)
                        return null;
                    TrdCommon.TrdHeader trdHeader = MakeTrdHeader(trdEnv, accID, trdMarket);
                    TrdGetPositionList.C2S.Builder c2s = TrdGetPositionList.C2S.CreateBuilder()
                            .SetHeader(trdHeader);
                    if (filterConditions != null)
                    {
                        c2s.SetFilterConditions(filterConditions);
                    }
                    if (filterPLRatioMin.HasValue)
                    {
                        c2s.SetFilterPLRatioMin(filterPLRatioMin.Value);
                    }
                    if (filterPLRatioMax.HasValue)
                    {
                        c2s.SetFilterPLRatioMax(filterPLRatioMax.Value);
                    }
                    c2s.SetRefreshCache(isRefreshCache);
                    TrdGetPositionList.Request req = TrdGetPositionList.Request.CreateBuilder().SetC2S(c2s).Build();
                    uint sn = trd.GetPositionList(req);
                    if (sn == 0)
                        return null;
                    reqInfo = new ReqInfo(ProtoID.TrdGetPositionList, syncEvent);
                    trdReqInfoMap.Add(sn, reqInfo);
                }
                Monitor.Wait(syncEvent);
                return (TrdGetPositionList.Response)reqInfo.Rsp;
            }

        }


        public void OnReply_GetSecuritySnapshot(FTAPI_Conn client, uint nSerialNo, QotGetSecuritySnapshot.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetSecuritySnapshot, rsp);
        }


        public void OnReply_GetStaticInfo(FTAPI_Conn client, uint nSerialNo, QotGetStaticInfo.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetStaticInfo, rsp);
        }



        public void OnReply_GetOrderBook(FTAPI_Conn client, uint nSerialNo, QotGetOrderBook.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetOrderBook, rsp);
        }


        public void OnReply_RequestHistoryKL(FTAPI_Conn client, uint nSerialNo, QotRequestHistoryKL.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotRequestHistoryKL, rsp);
        }


        public void OnReply_PlaceOrder(FTAPI_Conn client, uint nSerialNo, TrdPlaceOrder.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdPlaceOrder, rsp);
        }


        public void OnReply_UnlockTrade(FTAPI_Conn client, uint nSerialNo, TrdUnlockTrade.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdUnlockTrade, rsp);
        }


        public void OnReply_SubAccPush(FTAPI_Conn client, uint nSerialNo, TrdSubAccPush.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdSubAccPush, rsp);
        }


        public void OnReply_GetAccList(FTAPI_Conn client, uint nSerialNo, TrdGetAccList.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdGetAccList, rsp);
        }


        public void OnReply_GetFunds(FTAPI_Conn client, uint nSerialNo, TrdGetFunds.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdGetFunds, rsp);
        }


        public void OnReply_GetOrderList(FTAPI_Conn client, uint nSerialNo, TrdGetOrderList.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdGetOrderList, rsp);
        }


        public void OnReply_GetOrderFillList(FTAPI_Conn client, uint nSerialNo, TrdGetOrderFillList.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdGetOrderFillList, rsp);
        }


        public void OnReply_GetHistoryOrderList(FTAPI_Conn client, uint nSerialNo, TrdGetHistoryOrderList.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdGetHistoryOrderList, rsp);
        }


        public void OnReply_GetHistoryOrderFillList(FTAPI_Conn client, uint nSerialNo, TrdGetHistoryOrderFillList.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdGetHistoryOrderFillList, rsp);
        }


        public void OnReply_GetPositionList(FTAPI_Conn client, uint nSerialNo, TrdGetPositionList.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdGetPositionList, rsp);
        }

        ReqInfo getQotReqInfo(uint serialNo, ProtoID protoID)
        {
            lock (qotLock)
            {
                ReqInfo info;
                if (qotReqInfoMap.TryGetValue(serialNo, out info) && info.ProtoID == protoID)
                {
                    qotReqInfoMap.Remove(serialNo);
                    return info;
                }
            }
            return null;
        }

        ReqInfo getTrdReqInfo(uint serialNo, ProtoID protoID)
        {
            lock (trdLock)
            {
                ReqInfo info;
                if (trdReqInfoMap.TryGetValue(serialNo, out info) && info.ProtoID == protoID)
                {
                    trdReqInfoMap.Remove(serialNo);
                    return info;
                }
            }
            return null;
        }

        void handleQotOnReply(uint serialNo, ProtoID protoID, object rsp)
        {
            ReqInfo reqInfo = getQotReqInfo(serialNo, protoID);
            if (reqInfo != null)
            {
                lock (reqInfo.SyncEvent)
                {
                    reqInfo.Rsp = rsp;
                    Monitor.PulseAll(reqInfo.SyncEvent);
                }
            }
        }

        void handleTrdOnReply(uint serialNo, ProtoID protoID, object rsp)
        {
            ReqInfo reqInfo = getTrdReqInfo(serialNo, protoID);
            if (reqInfo != null)
            {
                lock (reqInfo.SyncEvent)
                {
                    reqInfo.Rsp = rsp;
                    Monitor.PulseAll(reqInfo.SyncEvent);
                }
            }
        }

        public static QotCommon.Security MakeSec(QotCommon.QotMarket market, String code)
        {
            QotCommon.Security sec = QotCommon.Security.CreateBuilder().SetCode(code)
                    .SetMarket((int)market)
                    .Build();
            return sec;
        }

        public static TrdCommon.TrdHeader MakeTrdHeader(TrdCommon.TrdEnv trdEnv,
                                                 ulong accID,
                                                 TrdCommon.TrdMarket trdMarket)
        {
            TrdCommon.TrdHeader header = TrdCommon.TrdHeader.CreateBuilder()
                    .SetTrdEnv((int)trdEnv)
                    .SetAccID(accID)
                    .SetTrdMarket((int)trdMarket)
                    .Build();
            return header;
        }

        public void OnReply_GetGlobalState(FTAPI_Conn client, uint nSerialNo, GetGlobalState.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.GetGlobalState, rsp);
        }

        public void OnReply_Sub(FTAPI_Conn client, uint nSerialNo, QotSub.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotSub, rsp);
        }

        public void OnReply_RegQotPush(FTAPI_Conn client, uint nSerialNo, QotRegQotPush.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotRegQotPush, rsp);
        }

        public void OnReply_GetSubInfo(FTAPI_Conn client, uint nSerialNo, QotGetSubInfo.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetSubInfo, rsp);
        }

        public void OnReply_GetTicker(FTAPI_Conn client, uint nSerialNo, QotGetTicker.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetTicker, rsp);
        }

        public void OnReply_GetBasicQot(FTAPI_Conn client, uint nSerialNo, QotGetBasicQot.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetBasicQot, rsp);
        }


        public void OnReply_GetKL(FTAPI_Conn client, uint nSerialNo, QotGetKL.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetKL, rsp);
        }

        public void OnReply_GetRT(FTAPI_Conn client, uint nSerialNo, QotGetRT.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetRT, rsp);
        }

        public void OnReply_GetBroker(FTAPI_Conn client, uint nSerialNo, QotGetBroker.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetBroker, rsp);
        }

        public void OnReply_GetHistoryKL(FTAPI_Conn client, uint nSerialNo, QotGetHistoryKL.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetHistoryKL, rsp);
        }

        public void OnReply_GetHistoryKLPoints(FTAPI_Conn client, uint nSerialNo, QotGetHistoryKLPoints.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetHistoryKLPoints, rsp);
        }

        public void OnReply_GetRehab(FTAPI_Conn client, uint nSerialNo, QotGetRehab.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetRehab, rsp);
        }

        public void OnReply_RequestRehab(FTAPI_Conn client, uint nSerialNo, QotRequestRehab.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotRequestRehab, rsp);
        }


        public void OnReply_RequestHistoryKLQuota(FTAPI_Conn client, uint nSerialNo, QotRequestHistoryKLQuota.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotRequestHistoryKLQuota, rsp);
        }

        public void OnReply_GetTradeDate(FTAPI_Conn client, uint nSerialNo, QotGetTradeDate.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetTradeDate, rsp);
        }


        public void OnReply_GetPlateSet(FTAPI_Conn client, uint nSerialNo, QotGetPlateSet.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetPlateSet, rsp);
        }

        public void OnReply_GetPlateSecurity(FTAPI_Conn client, uint nSerialNo, QotGetPlateSecurity.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetPlateSecurity, rsp);
        }

        public void OnReply_GetReference(FTAPI_Conn client, uint nSerialNo, QotGetReference.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetReference, rsp);
        }

        public void OnReply_GetOwnerPlate(FTAPI_Conn client, uint nSerialNo, QotGetOwnerPlate.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetOwnerPlate, rsp);
        }

        public void OnReply_GetHoldingChangeList(FTAPI_Conn client, uint nSerialNo, QotGetHoldingChangeList.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetHoldingChangeList, rsp);
        }

        public void OnReply_GetOptionChain(FTAPI_Conn client, uint nSerialNo, QotGetOptionChain.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetOptionChain, rsp);
        }

        public void OnReply_GetWarrant(FTAPI_Conn client, uint nSerialNo, QotGetWarrant.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetWarrant, rsp);
        }

        public void OnReply_GetCapitalFlow(FTAPI_Conn client, uint nSerialNo, QotGetCapitalFlow.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetCapitalFlow, rsp);
        }

        public void OnReply_GetCapitalDistribution(FTAPI_Conn client, uint nSerialNo, QotGetCapitalDistribution.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetCapitalDistribution, rsp);
        }

        public void OnReply_GetUserSecurity(FTAPI_Conn client, uint nSerialNo, QotGetUserSecurity.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetUserSecurity, rsp);
        }

        public void OnReply_ModifyUserSecurity(FTAPI_Conn client, uint nSerialNo, QotModifyUserSecurity.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotModifyUserSecurity, rsp);
        }

        public void OnReply_GetIpoList(FTAPI_Conn client, uint nSerialNo, QotGetIpoList.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetIpoList, rsp);
        }

        public void OnReply_GetFutureInfo(FTAPI_Conn client, uint nSerialNo, QotGetFutureInfo.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetFutureInfo, rsp);
        }

        public void OnReply_RequestTradeDate(FTAPI_Conn client, uint nSerialNo, QotRequestTradeDate.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotRequestTradeDate, rsp);
        }

        public void OnReply_Notify(FTAPI_Conn client, uint nSerialNo, Notify.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.Notify, rsp);
        }

        public void OnReply_StockFilter(FTAPI_Conn client, uint nSerialNo, QotStockFilter.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotStockFilter, rsp);
        }

        public void OnReply_GetCodeChange(FTAPI_Conn client, uint nSerialNo, QotGetCodeChange.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetCodeChange, rsp);
        }

        public void OnReply_SetPriceReminder(FTAPI_Conn client, uint nSerialNo, QotSetPriceReminder.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotSetPriceReminder, rsp);
        }

        public void OnReply_GetPriceReminder(FTAPI_Conn client, uint nSerialNo, QotGetPriceReminder.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetPriceReminder, rsp);
        }

        public void OnReply_GetUserSecurityGroup(FTAPI_Conn client, uint nSerialNo, QotGetUserSecurityGroup.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetUserSecurityGroup, rsp);
        }

        public void OnReply_GetMarketState(FTAPI_Conn client, uint nSerialNo, QotGetMarketState.Response rsp)
        {
            handleQotOnReply(nSerialNo, ProtoID.QotGetMarketState, rsp);
        }

        public virtual void OnReply_UpdateBasicQot(FTAPI_Conn client, uint nSerialNo, QotUpdateBasicQot.Response rsp)
        {

        }

        public virtual void OnReply_UpdateKL(FTAPI_Conn client, uint nSerialNo, QotUpdateKL.Response rsp)
        {

        }

        public virtual void OnReply_UpdateRT(FTAPI_Conn client, uint nSerialNo, QotUpdateRT.Response rsp)
        {

        }

        public virtual void OnReply_UpdateTicker(FTAPI_Conn client, uint nSerialNo, QotUpdateTicker.Response rsp)
        {

        }

        public virtual void OnReply_UpdateOrderBook(FTAPI_Conn client, uint nSerialNo, QotUpdateOrderBook.Response rsp)
        {

        }

        public virtual void OnReply_UpdateBroker(FTAPI_Conn client, uint nSerialNo, QotUpdateBroker.Response rsp)
        {

        }

        public virtual void OnReply_UpdateOrderDetail(FTAPI_Conn client, uint nSerialNo, QotUpdateOrderDetail.Response rsp)
        {

        }

        public virtual void OnReply_UpdatePriceReminder(FTAPI_Conn client, uint nSerialNo, QotUpdatePriceReminder.Response rsp)
        {

        }


        public void OnReply_GetMaxTrdQtys(FTAPI_Conn client, uint nSerialNo, TrdGetMaxTrdQtys.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdGetMaxTrdQtys, rsp);
        }


        public void OnReply_ModifyOrder(FTAPI_Conn client, uint nSerialNo, TrdModifyOrder.Response rsp)
        {
            handleTrdOnReply(nSerialNo, ProtoID.TrdModifyOrder, rsp);
        }


        public virtual void OnReply_UpdateOrder(FTAPI_Conn client, uint nSerialNo, TrdUpdateOrder.Response rsp)
        {

        }

        public virtual void OnReply_UpdateOrderFill(FTAPI_Conn client, uint nSerialNo, TrdUpdateOrderFill.Response rsp)
        {

        }
    }
}

