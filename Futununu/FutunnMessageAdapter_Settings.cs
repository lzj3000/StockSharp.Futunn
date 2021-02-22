using Ecng.Serialization;
using StockSharp.Localization;
using StockSharp.Messages;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// 上海交易所
        /// </summary>
        ShangHai = 21,
        /// <summary>
        /// 深圳交易所
        /// </summary>
        ShenZhen = 22
    }
    [CategoryLoc("China")]
    [DescriptionLoc("Str1770", "Futununu")]
    [DisplayNameLoc("Futununu")]
    //[MediaIcon("Futununu_logo.png")]
    [MessageAdapterCategory(MessageAdapterCategories.China | MessageAdapterCategories.Stock 
        | MessageAdapterCategories.RealTime | MessageAdapterCategories.Free 
        | MessageAdapterCategories.Ticks | MessageAdapterCategories.Candles 
        | MessageAdapterCategories.MarketDepth | MessageAdapterCategories.Level1 
        | MessageAdapterCategories.Transactions)]
    public partial class FutunnMessageAdapter :ILoginPasswordAdapter
    {
        /// <summary>
        /// 版本号
        /// </summary>
        //[Display(ResourceType = typeof(LocalizedStrings), Name = "版本", Description = "futu API 版本", GroupName = "General", Order = 0)]
        //public FutununVersions Version { get; set; }
        /// <summary>
        /// 国内股票市场
        /// </summary>
        [Display(ResourceType = typeof(LocalizedStrings), Name = "股票市场", Description = "国内的股票市场选择", GroupName = "General", Order = 1)]
        public StockMarket StockMarket { get; set; }
        /// <summary>
        /// 登录名
        /// </summary>
        [Display(ResourceType = typeof(LocalizedStrings), Name = "平台账号", Description = "平台账号是您在富途的用户 ID（包括牛牛号和 moomoo 号）", GroupName = "Str174", Order = 1)]
        public string Login { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Display(ResourceType = typeof(LocalizedStrings), Name = "密码", Description = "Str2262", GroupName = "Str174", Order = 2)]
        public SecureString Password { get; set; }
        /// <summary>
        /// OpendIP
        /// </summary>
        [Display(ResourceType = typeof(LocalizedStrings), Name = "网关IP", Description = "Str2262", GroupName = "Str174", Order = 3)]
        public string OpendIP { get; set; }
        /// <summary>
        /// OpendPort
        /// </summary>
        [Display(ResourceType = typeof(LocalizedStrings), Name = "网关端口", Description = "Str2262", GroupName = "Str174", Order = 4)]
        public ushort OpendPort { get; set; }

        [Display(ResourceType = typeof(LocalizedStrings), Name = "交易业务账户", Description = "交易业务账户是您的证券账户，需要完成相应券商的开户流程才能得到，主要用于出入金、融资融券、交易各类金融标的、直接持有资产和持仓。",
            GroupName = "Str174", Order = 5)]
        public ulong TrdAcc { get; set; }


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
