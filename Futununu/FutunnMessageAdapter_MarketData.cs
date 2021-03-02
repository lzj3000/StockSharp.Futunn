
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

		private void ProcessTime(Message message)
		{
			foreach (var code in subListTypes.Keys.ToArray())
			{
				var dataTypes = subListTypes[code];
				foreach (var datatype in dataTypes) {

					if (datatype == DataType.MarketDepth)
					{
						var msg = new QuoteChangeMessage();
						var books = market.GetOrderBook(code, 10);
						var bids = new List<QuoteChange>();
						foreach (var bid in books.S2C.OrderBookBidListList)
							bids.Add(new QuoteChange((decimal)bid.Price, bid.Volume));
						msg.Bids = bids.ToArray();
						var asks = new List<QuoteChange>();
						foreach (var ask in books.S2C.OrderBookAskListList)
							asks.Add(new QuoteChange((decimal)ask.Price, ask.Volume));
						msg.Asks = asks.ToArray();
						msg.ServerTime = Convert.ToDateTime(books.S2C.SvrRecvTimeAsk);
						
						
					}
					if (datatype == DataType.Ticks)
					{
						var msg = new ExecutionMessage();
						var tick = market.GetTicker(code);
						foreach (var tk in tick.S2C.TickerListList) {
							msg.ExecutionType = ExecutionTypes.Tick;
							msg.TradePrice = (decimal)tk.Price;
							msg.TradeVolume = tk.Volume;
							msg.ServerTime = Convert.ToDateTime(tk.Time);
							msg.OriginSide =tk.Dir==1 ? Sides.Buy : Sides.Sell;
							sendOutMessage(code, msg);
						}
					}
					if (datatype == DataType.Level1)
					{
						var msg = new Level1ChangeMessage();
						var qot = market.GetBasicQot(code).S2C.GetBasicQotList(0);
						msg.TryAdd(Level1Fields.LastTradeTime, qot.UpdateTime);
						msg.TryAdd(Level1Fields.LastTradePrice, (decimal)qot.LastClosePrice);
						msg.TryAdd(Level1Fields.Volume, qot.Volume);
						msg.TryAdd(Level1Fields.OpenPrice, (decimal)qot.OpenPrice);
						msg.TryAdd(Level1Fields.LowPrice, (decimal)qot.LowPrice);
						msg.TryAdd(Level1Fields.MaxPrice, (decimal)qot.HighPrice);
						msg.TryAdd(Level1Fields.MinPrice, (decimal)qot.LowPrice);
						sendOutMessage(code, msg);
					}
				}
			}
		}
		private Dictionary<string, MarketDataMessage> securityList = new Dictionary<string, MarketDataMessage>();
		private Dictionary<string, List<DataType>> subListTypes = new Dictionary<string, List<DataType>>();

		private void SubscribeSecurityInfo(MarketDataMessage mdMsg)
		{
			var code = mdMsg.SecurityId.SecurityCode;
			if (mdMsg.IsSubscribe)
			{
			
				if (!securityList.ContainsKey(code))
					securityList.Add(mdMsg.SecurityId.SecurityCode, mdMsg);
				else
					securityList[code] = mdMsg;
				if (!subListTypes.ContainsKey(code))
					subListTypes.Add(code, new List<DataType>());
				if (!subListTypes[code].Contains(mdMsg.DataType2))
					subListTypes[code].Add(mdMsg.DataType2);
			}
			else
			{
				if (subListTypes.ContainsKey(code)) {
					if (subListTypes[code].Contains(mdMsg.DataType2)) {
						subListTypes[code].Remove(mdMsg.DataType2);
					}
					if (subListTypes[code].Count == 0) {
						subListTypes.Remove(code);
					}
				 }
			}
		}

		private void sendOutMessage(string code, Message message) {
			if (securityList.ContainsKey(code))
			{
				var msg = securityList[code];
				message.ReplaceSecurityId(msg.SecurityId);
			}
			SendOutMessage(message);
			
		}
		private void OnSendOutMessage(Level1ChangeMessage message, DataType dataType)
		{
			if (dataType == DataType.MarketDepth) { 
			    
			}
			if (dataType == DataType.Ticks)
			{

			}
			if (dataType == DataType.CandleTimeFrame)
			{

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
				sendOutMessage(obj.S2C.Security.Code,new ExecutionMessage
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
				//secMsg.AddValue(nameof(Level1Fields.LastTradeId),);

				//secMsg.AddValue(nameof(Level1Fields.LastTradeVolume),);
				//secMsg.AddValue(nameof(Level1Fields.BestBidPrice), );
				//secMsg.AddValue(nameof(Level1Fields.BidsVolume),);
				//secMsg.AddValue(nameof(Level1Fields.BestAskPrice),);

				//secMsg.AddValue(nameof(Level1Fields.OpenInterest), );

				//secMsg.AddValue(nameof(Level1Fields.ImpliedVolatility), );
				//secMsg.AddValue(nameof(Level1Fields.HistoricalVolatility), );

				l1.TryAdd(Level1Fields.LastTradeTime, qot.UpdateTime);
				l1.TryAdd(Level1Fields.LastTradePrice, (decimal)qot.LastClosePrice);
				l1.TryAdd(Level1Fields.Volume, qot.Volume);
				l1.TryAdd(Level1Fields.OpenPrice, (decimal)qot.OpenPrice);
				l1.TryAdd(Level1Fields.LowPrice, (decimal)qot.LowPrice);
				l1.TryAdd(Level1Fields.MaxPrice, (decimal)qot.HighPrice);
				l1.TryAdd(Level1Fields.MinPrice, (decimal)qot.LowPrice);
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
				
				
				SendOutMessage(secMsg);
			}
			SendSubscriptionResult(lookupMsg);
		}
		
		protected override IEnumerable<TimeSpan> GetTimeFrames(SecurityId securityId, DateTimeOffset? from, DateTimeOffset? to)
		{
			HashSet<TimeSpan> _timeFrames = new HashSet<TimeSpan>();
			
			_timeFrames.Add(TimeSpan.FromMinutes(1));
			_timeFrames.Add(TimeSpan.FromMinutes(3));
			_timeFrames.Add(TimeSpan.FromMinutes(5));
			_timeFrames.Add(TimeSpan.FromMinutes(15));
			_timeFrames.Add(TimeSpan.FromMinutes(30));
			_timeFrames.Add(TimeSpan.FromMinutes(60));
			_timeFrames.Add(TimeSpan.FromDays(1));
			_timeFrames.Add(TimeSpan.FromDays(7));
			_timeFrames.Add(TimeSpan.FromDays(30));
			_timeFrames.Add(TimeSpan.FromDays(90));
			_timeFrames.Add(TimeSpan.FromDays(365));
			return _timeFrames;
		}
		public override IEnumerable<object> GetCandleArgs(Type candleType, SecurityId securityId, DateTimeOffset? from, DateTimeOffset? to)
		{
			TimeFrameCandleMessage tfcm = new TimeFrameCandleMessage();
			
			return base.GetCandleArgs(candleType, securityId, from, to);
		}
	}
}
