
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
			
		    if (mdMsg.DataType2 == DataType.MarketDepth)
			{
				if (mdMsg.IsSubscribe)
				{

				}
				else { 
				
				}
			}
			if (mdMsg.DataType2 == DataType.Ticks)
			{

			}

			SendSubscriptionReply(mdMsg.TransactionId);
		}
		private void ProcessSecurityLookup(SecurityLookupMessage lookupMsg)
		{
			MarketData market = new MarketData(OpendIP, OpendPort);
			var secTypes = lookupMsg.GetSecurityTypes();
			List<SecurityType> securityTypeList = new List<SecurityType>();
			foreach (var sectype in secTypes) {
				securityTypeList.Add(sectype.Conver());
			}
			var securities = market.GetSecurityList(securityTypeList.ToArray());
			foreach(var sec in securities) {
				var secMsg = sec.Conver();
				secMsg.OriginalTransactionId = lookupMsg.TransactionId;
				if (!secMsg.IsMatch(lookupMsg, secTypes))
					continue;
				SendOutMessage(secMsg);
			}
			SendSubscriptionResult(lookupMsg);
		}
	}
}
