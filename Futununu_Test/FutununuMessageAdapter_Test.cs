using Ecng.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockSharp.Futunn;
using StockSharp.Messages;
using System.Security;

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
            adapter.Login = "7715583";
            char[] psc = "8211101Lxc".ToCharArray();
            adapter.Password = new SecureString();
            foreach (var c in psc)
                adapter.Password.AppendChar(c);
            adapter.NewOutMessage += Adapter_NewOutMessage;
        }
        [TestMethod]
        public void Connect_Test()
        {
            adapter.SendInMessage(new ConnectMessage());
        }
        [TestMethod]
        public void CloseConnect_Test()
        {

        }

        private void Adapter_NewOutMessage(Message obj)
        {
            ConsoleHelper.ConsoleInfo(obj.ToString());
        }
    }
}
