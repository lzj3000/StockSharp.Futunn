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
            var mes = new SecurityLookupMessage();
            mes.SecurityTypes = new SecurityTypes[] {
                SecurityTypes.Stock,
                SecurityTypes.Index
            };
            adapter.SendInMessage(mes);
            Thread.Sleep(200);
        }
        [TestMethod]
        public void MarketData_Test()
        {

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

        private void Adapter_NewOutMessage(Message obj)
        {
            Console.WriteLine(obj.ToString());
        }
    }
}
