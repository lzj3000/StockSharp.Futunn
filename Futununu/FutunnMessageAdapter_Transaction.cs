using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockSharp.Futunn
{
    partial class FutunnMessageAdapter
    {

		private void ProcessOrderRegister(OrderRegisterMessage regMsg)
		{
			var price = regMsg.Price;
			if (regMsg.OrderType == OrderTypes.Market) {
				var ob = market.GetOrderBook(regMsg.SecurityId.SecurityCode);
				if (regMsg.Side == Sides.Buy)
				{
					price = (decimal)ob.S2C.GetOrderBookBidList(0).Price;
				}
				else {
					price = (decimal)ob.S2C.GetOrderBookAskList(0).Price;
				}
			}
			transaction.OrderRegister(regMsg.SecurityId.SecurityCode,
				(double)regMsg.Volume,
				(double)price,
				regMsg.Side == Sides.Buy ? 1 : 2);

		}

		private void ProcessOrderCancel(OrderCancelMessage cancelMsg)
		{
			
		}

		private void ProcessOrder(object order, decimal balance, long transId, long origTransId)
		{
		
		}

		private void ProcessTrade(object transaction)
		{
			
		}

		private void ProcessOrderStatus(OrderStatusMessage message)
		{
			
		}



		private void ProcessPortfolioLookup(PortfolioLookupMessage message)
		{
		}
	}
}
