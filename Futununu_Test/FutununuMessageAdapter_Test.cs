using Ecng.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockSharp.Futunn;
using StockSharp.Messages;
using System;
using System.Diagnostics;
using System.Security;
using System.Threading;

namespace Futununu_Test
{
    [TestClass]
    public class FutununuMessageAdapter_Test
    {
      
        FutunnMessageAdapter adapter;
        public FutununuMessageAdapter_Test()
        {
            adapter = new FutunnMessageAdapter(new IncrementalIdGenerator());
            adapter.StockMarket = StockMarket.CNSH_Security;
            adapter.OpendIP = "127.0.0.1";
            adapter.OpendPort = 11111;
            adapter.Login = "123456";
            char[] psc = "123456".ToCharArray();
            adapter.Password = new SecureString();
            foreach (var c in psc)
                adapter.Password.AppendChar(c);
            adapter.NewOutMessage += Adapter_NewOutMessage;
            
        }
        [TestMethod]
        public void Connect_Test()
        {
            Console.WriteLine("Test Connect");
            adapter.SendInMessage(new ConnectMessage());
            Thread.Sleep(200);
        }
        [TestMethod]
        public void SecurityLookup_Test()
        {

            Console.WriteLine("Test SecurityLookup");
            var meg = new SecurityLookupMessage();
            meg.SecurityTypes = new SecurityTypes[] {
                SecurityTypes.Stock
            };
            adapter.SendInMessage(meg);
            Thread.Sleep(200);
        }
        [TestMethod]
        public void MarketData_Test()
        {
            Console.WriteLine("Test MarketData");
            var msg = new MarketDataMessage();
            msg.IsSubscribe = true;
            msg.SecurityId =new SecurityId() { SecurityCode= "688185" };
            msg.DataType2 = DataType.MarketDepth;
            adapter.SendInMessage(msg);
            Thread.Sleep(3000);
        }
        [TestMethod]
        public void OrderRegister_Test()
        {

        }
        [TestMethod]
        public void OrderStatus_Test()
        {

        }
        [TestMethod]
        public void PortfolioLookup_Test()
        {

        }
        SecurityMessage security;
        private void Adapter_NewOutMessage(Message obj)
        {
            Console.WriteLine(obj.IsBack());
            if (obj is SecurityMessage) {
                if (security == null)
                    security = obj as SecurityMessage;
            }
            Console.WriteLine(obj.ToString());
        }
    }
}
