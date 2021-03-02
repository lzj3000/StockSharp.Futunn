using Ecng.Common;
using StockSharp.Futunn.Native.mapping;
using StockSharp.Localization;
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
            transaction.Error += Transaction_Error;
		}

        private void Transaction_Error(Exception obj)
        {
			SendOutError(obj);
        }

        private void Transaction_OrderStates(Futu.OpenApi.Pb.TrdUpdateOrder.Response obj)
		{
			var order = obj.S2C.Order;
			if (securityTradeList.ContainsKey(order.Code))
			{
				var reg = securityTradeList[order.Code];
				reg.Volume = (decimal)order.Qty;
				reg.Price = (decimal)order.Price;
				ProcessOrderStatus(reg, (long)order.OrderID, OrderStates.Active, Convert.ToDateTime(order.UpdateTimestamp));
			}
		}
		private void Transaction_OrderTradeFill(Futu.OpenApi.Pb.TrdUpdateOrderFill.Response obj)
		{	
			var order = obj.S2C.OrderFill;
			if (securityTradeList.ContainsKey(order.Code)) {
				securityTradeList.Remove(order.Code);
				var reg = securityTradeList[order.Code];
				reg.Volume = (decimal)order.Qty;
				reg.Price = (decimal)order.Price;
				ProcessOrderStatus(reg, (long)order.OrderID, OrderStates.Done, Convert.ToDateTime(order.UpdateTimestamp));
			}
		}
		private void ProcessOrderStatus(OrderRegisterMessage message,long orderid, OrderStates state,DateTime updateTime)
		{
			var msg = message.ToExec();
			msg.OrderState = state;
			msg.OrderId = orderid;
			msg.ServerTime = updateTime;
			SendOutMessage(msg);
		}
		private readonly Dictionary<string, OrderRegisterMessage> securityTradeList = new Dictionary<string, OrderRegisterMessage>();
		private void ProcessOrderRegister(OrderRegisterMessage regMsg)
		{
			var price = regMsg.Price;
			if (regMsg.OrderType == OrderTypes.Market) {
				var ob = market.GetOrderBook(regMsg.SecurityId.SecurityCode,1);
				if (regMsg.Side == Sides.Buy)
				{
					price = (decimal)ob.S2C.GetOrderBookBidList(0).Price;
				}
				else {
					price = (decimal)ob.S2C.GetOrderBookAskList(0).Price;
				}
			}
			if (!securityTradeList.ContainsKey(regMsg.SecurityId.SecurityCode))
			{
				securityTradeList.Add(regMsg.SecurityId.SecurityCode, regMsg);
				transaction.IsDemo = IsDemo;
				transaction.OrderRegister(regMsg.SecurityId.SecurityCode,
					(double)regMsg.Volume,
					(double)price,
					regMsg.Side == Sides.Buy ? 1 : 2);
			}

		}

		private void ProcessOrderCancel(OrderCancelMessage cancelMsg)
		{
			if (cancelMsg.OrderId == null)
				throw new InvalidOperationException(LocalizedStrings.Str2252Params.Put(cancelMsg.OriginalTransactionId));
			var res = transaction.OrderCancelSync((ulong)cancelMsg.OrderId);
			if (securityTradeList.ContainsKey(cancelMsg.SecurityId.SecurityCode))
			{
				securityTradeList.Remove(cancelMsg.SecurityId.SecurityCode);
			}
		}


		
		private void ProcessPortfolioLookup(PortfolioLookupMessage message)
		{
			if (message != null)
			{
				if (!message.IsSubscribe)
					return;
			}

			var transactionId = message?.TransactionId ?? 0;
			var pfName = "Stock";
			SendOutMessage(new PortfolioMessage
			{
				PortfolioName= pfName,
				BoardCode = Extensions.FutunnBoard,
				OriginalTransactionId = transactionId,
			});
			if (message != null)
				SendSubscriptionResult(message);
			var res = transaction.GetPositionList();
			foreach (var position in res.S2C.PositionListList)
			{
				var msg = this.CreatePositionChangeMessage(pfName,new SecurityId() { SecurityCode=position.Code,BoardCode=position.SecMarket.ToString()});
				msg.Side = ((TrdSide)position.PositionSide).Convert();
				msg.SubscriptionId = (long)position.PositionID;
				msg.TryAdd(PositionChangeTypes.BeginValue, (decimal)position.Val, true);
				msg.TryAdd(PositionChangeTypes.CurrentValue, (decimal)position.Price, true);
				msg.TryAdd(PositionChangeTypes.BlockedValue, (decimal)position.Qty, true);
				msg.TryAdd(PositionChangeTypes.CurrentValue, (decimal)position.Val, true);
				msg.TryAdd(PositionChangeTypes.CurrentPrice, (decimal)position.Price, true);
				msg.TryAdd(PositionChangeTypes.TradesCount, (decimal)position.Qty, true);
				SendOutMessage(msg);
			}
		}
	}
}
