
using StockSharp.Futunn.Native;
using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Futu.OpenApi.Pb.QotCommon;
using StockSharp.Futunn.Native.mapping;
using System.Threading;

namespace StockSharp.Futunn
{
    partial class FutunnMessageAdapter
    {
		private void ProcessMarketData(MarketDataMessage mdMsg)
		{
			SubscribeSecurityInfo(mdMsg);
			SendSubscriptionReply(mdMsg.TransactionId);
		}
		private Dictionary<string, MarketDataMessage> securityList = new Dictionary<string, MarketDataMessage>();

		private void SubscribeSecurityInfo(MarketDataMessage mdMsg)
		{
			if (mdMsg.IsSubscribe)
			{
				if (!securityList.ContainsKey(mdMsg.SecurityId.SecurityCode))
				{
					securityList.Add(mdMsg.SecurityId.SecurityCode, mdMsg);
				}
				if (mdMsg.DataType2 == DataType.MarketDepth)
				{
					
				}
				if (mdMsg.DataType2 == DataType.Ticks) { 
				
				}
				if (mdMsg.DataType2 == DataType.OrderLog) { 
				
				}
				
			}
			else
			{
				market.SubAllInfo(new string[] { mdMsg.SecurityId.SecurityCode }, false);
				securityList.Remove(mdMsg.SecurityId.SecurityCode);
			}
		}
		
		private void SubscribeMarketInfo() {
            market.BasicQotCallback += Market_BasicQotCallback;
			market.OrderBookCallback += Market_OrderBookCallback;
            market.KLCallback += Market_KLCallback;
            market.RTCallback += Market_RTCallback;
            market.TickerCallback += Market_TickerCallback;
		}

		private void UnSubscribeMarketInfo()
		{
			market.BasicQotCallback -= Market_BasicQotCallback;
			market.OrderBookCallback -= Market_OrderBookCallback;
			market.KLCallback -= Market_KLCallback;
			market.RTCallback -= Market_RTCallback;
			market.TickerCallback -= Market_TickerCallback;
		}

	    
        private void Market_TickerCallback(Futu.OpenApi.Pb.QotUpdateTicker.Response obj)
        {
			foreach (var ticker in obj.S2C.TickerListList)
			{
				SendOutMessage(new ExecutionMessage
				{
					ExecutionType = ExecutionTypes.Tick,
					SecurityId = new SecurityId() { SecurityCode = obj.S2C.Security.Code },
					TradeId = ticker.Sequence,
					TradePrice = (decimal)ticker.Price,
					TradeVolume = ticker.Volume,
					ServerTime = Convert.ToDateTime(ticker.Time),
					OriginSide = ticker.Dir==2?Sides.Buy:Sides.Sell,
				});
			}
        }

        private void Market_RTCallback(Futu.OpenApi.Pb.QotUpdateRT.Response obj)
        {
			var code=obj.S2C.Security.Code;
			foreach (var rt in obj.S2C.RtListList) {
				SendOutMessage(new TimeFrameCandleMessage()
				{
					TimeFrame = TimeSpan.Parse(rt.Timestamp.ToString()),
					
				});
			}
        }

		private void Market_KLCallback(Futu.OpenApi.Pb.QotUpdateKL.Response obj)
		{
			foreach (var kline in obj.S2C.KlListList)
			{
				SendOutMessage(new VolumeCandleMessage()
				{
					ClosePrice = (decimal)kline.ClosePrice,
					HighPrice = (decimal)kline.HighPrice,
					OpenPrice = (decimal)kline.OpenPrice,
					LowPrice = (decimal)kline.LowPrice,
					Volume = kline.Volume
					//kline.LastClosePrice
					//kline.Turnover 成交额
					//kline.TurnoverRate 换手率
					//kline.Pe 市盈率
					//kline.ChangeRate 涨跌幅
				});
			}
		}

        private void Market_BasicQotCallback(Futu.OpenApi.Pb.QotUpdateBasicQot.Response obj)
        {
			foreach (var qot in obj.S2C.BasicQotListList)
			{
				var l1 = new Level1ChangeMessage()
				{
					SecurityId = new SecurityId() { SecurityCode = qot.Security.Code },
				};
				l1.Changes.Add(Level1Fields.Volume, qot.Volume);
				l1.Changes.Add(Level1Fields.OpenPrice, qot.OpenPrice);
				l1.Changes.Add(Level1Fields.LowPrice, qot.LowPrice);
				l1.Changes.Add(Level1Fields.MaxPrice, qot.HighPrice);
				l1.Changes.Add(Level1Fields.MinPrice, qot.LowPrice);
				SendOutMessage(l1);
			}
        }

		private void Market_OrderBookCallback(Futu.OpenApi.Pb.QotUpdateOrderBook.Response obj)
		{
			SendOutMessage(new QuoteChangeMessage
			{
				SecurityId = new SecurityId() { SecurityCode = obj.S2C.Security.Code },
				Bids = obj.S2C.OrderBookBidListList.Select(e => new QuoteChange(Convert.ToDecimal(e.Price), e.Volume, e.OrederCount)).ToArray(),
				Asks = obj.S2C.OrderBookAskListList.Select(e => new QuoteChange(Convert.ToDecimal(e.Price), e.Volume, e.OrederCount)).ToArray(),
				ServerTime = Convert.ToDateTime(obj.S2C.SvrRecvTimeAskTimestamp)
			}); 
		}

        private void ProcessSecurityLookup(SecurityLookupMessage lookupMsg)
		{
			var secTypes = lookupMsg.GetSecurityTypes();
			List<SecurityType> securityTypeList = new List<SecurityType>();
			foreach (var sectype in secTypes) {
				securityTypeList.Add(sectype.Convert());
			}
			if (securityTypeList.Count == 0) {
				securityTypeList.Add(SecurityType.SecurityType_Eqty);
			}
			var securities = market.GetSecurityList(securityTypeList.ToArray());
			for(var i=0;i<10; i++) {
				var sec = securities[i];
				var secMsg = sec.Convert();
				secMsg.OriginalTransactionId = lookupMsg.TransactionId;
				
				var qot=market.GetSnapshots(secMsg.SecurityId.SecurityCode);
				secMsg.AddValue(nameof(Level1Fields.LastTradePrice), (decimal)qot.Basic.LastClosePrice);
				secMsg.AddValue(nameof(Level1Fields.BestBidPrice), (decimal)qot.Basic.BidPrice);
				secMsg.AddValue(nameof(Level1Fields.BidsVolume), (decimal)qot.Basic.BidVol);
				secMsg.AddValue(nameof(Level1Fields.BestAskPrice), (decimal)qot.Basic.AskPrice);
				secMsg.AddValue(nameof(Level1Fields.AsksVolume), (decimal)qot.Basic.AskVol);
				SendOutMessage(secMsg);
			}
			SendSubscriptionResult(lookupMsg);
		}

		protected override IEnumerable<TimeSpan> GetTimeFrames(SecurityId securityId, DateTimeOffset? from, DateTimeOffset? to)
		{
			return base.GetTimeFrames(securityId, from, to);
		}
		public override IEnumerable<object> GetCandleArgs(Type candleType, SecurityId securityId, DateTimeOffset? from, DateTimeOffset? to)
		{
			return base.GetCandleArgs(candleType, securityId, from, to);
		}
	}
}
