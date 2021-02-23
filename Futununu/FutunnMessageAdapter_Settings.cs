using Ecng.ComponentModel;
using Ecng.Serialization;
using StockSharp.Futunn.component;
using StockSharp.Localization;
using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security;
using System.Text;

namespace StockSharp.Futunn
{
    /// <summary>
    /// 国内股票市场
    /// </summary>
    public enum StockMarket
    {
        HK_Security = 1, //港股
        US_Security = 11, //美股
        CNSH_Security = 21, //沪股
        CNSZ_Security = 22 //深股
    }
    [CategoryLoc(LocalizedStrings.StockExchangeKey)]
    [DescriptionLoc("Str1770", "Futunn")]
    [DisplayNameLoc("Futunn")]
    [FutunnIcon("Futunn_logo.png")]
    [MessageAdapterCategory(MessageAdapterCategories.Asia | MessageAdapterCategories.Stock 
        | MessageAdapterCategories.RealTime | MessageAdapterCategories.Free 
        | MessageAdapterCategories.Ticks | MessageAdapterCategories.Candles 
        | MessageAdapterCategories.MarketDepth | MessageAdapterCategories.Level1 
        | MessageAdapterCategories.Transactions)]
    public partial class FutunnMessageAdapter :ILoginPasswordAdapter,IDemoAdapter
    {
        /// <summary>
        /// 版本号
        /// </summary>
        //[Display(ResourceType = typeof(LocalizedStrings), Name = "版本", Description = "futu API 版本", GroupName = "General", Order = 0)]
        //public FutununVersions Version { get; set; }
        /// <summary>
        /// 国内股票市场
        /// </summary>
        [Display(Name = "股票市场", Description = "国内的股票市场选择", GroupName = "General", Order = 1)]
        public StockMarket StockMarket { get; set; }
        /// <summary>
        /// 登录名
        /// </summary>
        [Display (Name = "平台账号", Description = "平台账号是您在富途的用户 ID（包括牛牛号和 moomoo 号）", GroupName = "Str174", Order = 1)]
        public string Login { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Display(Name = "密码", Description = "Str2262", GroupName = "Str174", Order = 2)]
        public SecureString Password { get; set; }
        /// <summary>
        /// OpendIP
        /// </summary>
        [Display( Name = "网关IP", Description = "Str2262", GroupName = "Str174", Order = 3)]
        public string OpendIP { get; set; }
        /// <summary>
        /// OpendPort
        /// </summary>
        [Display( Name = "网关端口", Description = "Str2262", GroupName = "Str174", Order = 4)]
        public ushort OpendPort { get; set; }
        /// <summary>
        /// 交易业务账户
        /// </summary>
        [Display( Name = "交易业务账户", Description = "交易业务账户是您的证券账户，需要完成相应券商的开户流程才能得到，主要用于出入金、融资融券、交易各类金融标的、直接持有资产和持仓。",
            GroupName = "Str174", Order = 5)]
        public ulong TrdAcc { get; set; }
        /// <summary>
        /// RSA私钥文件路径，用于加密和OpenD的连接
        /// </summary>
        [Display( Name = "私钥路径", Description = "RSA私钥文件路径，用于加密和OpenD的连接.",
           GroupName = "Str174", Order = 6)]
        [Editor(typeof(IFolderBrowserEditor), typeof(IFolderBrowserEditor))]
        public string RsaKeyFilePath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "是否Demo", Description = "是否Demo.",
          GroupName = "Str174", Order = 6)]
        public bool IsDemo { get ; set ; }

        /// <inheritdoc />
        public override void Save(SettingsStorage storage)
        {
            base.Save(storage);
            storage.SetValue(nameof(StockMarket), StockMarket);
            storage.SetValue(nameof(Login), Login);
            storage.SetValue(nameof(Password), Password);
            storage.SetValue(nameof(OpendIP), OpendIP);
            storage.SetValue(nameof(OpendPort), OpendPort);
            storage.SetValue(nameof(TrdAcc), TrdAcc);
        }

        /// <inheritdoc />
        public override void Load(SettingsStorage storage)
        {
            base.Load(storage);
            StockMarket= storage.GetValue<StockMarket>(nameof(StockMarket));
            Login = storage.GetValue<string>(nameof(Login));
            Password = storage.GetValue<SecureString>(nameof(Password));
            OpendIP = storage.GetValue<string>(nameof(OpendIP));
            OpendPort = storage.GetValue<ushort>(nameof(OpendPort));
            TrdAcc = storage.GetValue(nameof(TrdAcc), TrdAcc);
        }

    }
}
