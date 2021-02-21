using StockSharp.Futunn.Native.mapping;
using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using static Futu.OpenApi.Pb.TrdCommon;

namespace StockSharp.Futunn
{
    partial class FutunnMessageAdapter
    {
		private void SubscribeTransactionInfo()
		{
			transaction.OrderStates += Transaction_OrderStates;
            transaction.OrderTradeFill += Transaction_OrderTradeFill;
		}

		private void Transaction_OrderStates(Futu.OpenApi.Pb.TrdUpdateOrder.Response obj)
		{
			var order = obj.S2C.Order;
			if (orderList.ContainsKey(order.Code))
			{
				var reg = orderList[order.Code];
				var orderMsg = reg.ToExec();
				orderMsg.OrderId = (long)order.OrderID;
				orderMsg.OrderPrice = (decimal)order.Price;
				orderMsg.OrderVolume = (decimal)order.Qty;
				orderMsg.Side = ((TrdSide)order.TrdSide).Convert();
				orderMsg.AveragePrice = (decimal)order.FillAvgPrice;
				orderMsg.OrderState = OrderStates.Active;
				orderMsg.ServerTime = Convert.ToDateTime(order.UpdateTimestamp);
				SendOutMessage(orderMsg);
			}
		}
		private void Transaction_OrderTradeFill(Futu.OpenApi.Pb.TrdUpdateOrderFill.Response obj)
		{
			var order = obj.S2C.OrderFill;
			if (orderList.ContainsKey(order.Code)) {
				orderList.Remove(order.Code);
				var reg = orderList[order.Code];
				var orderMsg = reg.ToExec();
				orderMsg.OrderState = OrderStates.Done;
				orderMsg.OrderId = (long)order.OrderID;
				orderMsg.OrderPrice = (decimal)order.Price;
				orderMsg.OrderVolume = (decimal)order.Qty;
				orderMsg.Side = ((TrdSide)order.TrdSide).Convert();
				orderMsg.ServerTime = Convert.ToDateTime(order.UpdateTimestamp);
				SendOutMessage(orderMsg);
			}
		}
		private readonly Dictionary<string, OrderRegisterMessage> orderList = new Dictionary<string, OrderRegisterMessage>();
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
			if (!orderList.ContainsKey(regMsg.SecurityId.SecurityCode))
			{
				orderList.Add(regMsg.SecurityId.SecurityCode, regMsg);
				transaction.OrderRegister(regMsg.SecurityId.SecurityCode,
					(double)regMsg.Volume,
					(double)price,
					regMsg.Side == Sides.Buy ? 1 : 2);
			}

		}

		private void ProcessOrderCancel(OrderCancelMessage cancelMsg)
		{
			
		}

		private void ProcessPortfolioLookup(PortfolioLookupMessage message)
		{
		}
	}
}
