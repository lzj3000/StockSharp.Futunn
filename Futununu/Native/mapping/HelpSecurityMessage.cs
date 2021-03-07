using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using static Futu.OpenApi.Pb.QotCommon;

namespace StockSharp.Futunn.Native.Mapping
{
    /// <summary>
    /// Help for SecurityMessage
    /// </summary>
    public static class HelpSecurityMessage
    {
        /// <summary>
        /// Convert from SecurityStaticInfo to SecurityMessage
        /// </summary>
        /// <param name="info">SecurityStaticInfo</param>
        /// <returns></returns>
        public static SecurityMessage Convert(this SecurityStaticInfo info)
        {
            return new SecurityMessage()
            {
                SecurityId = new SecurityId() { SecurityCode = info.Basic.Security.Code, BoardCode = Extensions.FutunnBoard },
                SecurityType = ((SecurityType)info.Basic.SecType).Convert(),
                Name = info.Basic.Name,
                MinVolume = info.Basic.LotSize,
                VolumeStep = info.Basic.LotSize,
            };
        }
    }
}
