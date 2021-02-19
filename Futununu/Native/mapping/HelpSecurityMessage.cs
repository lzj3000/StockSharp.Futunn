using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using static Futu.OpenApi.Pb.QotCommon;

namespace StockSharp.Futunn.Native.mapping
{
   public static class HelpSecurityMessage
    {
        public static SecurityMessage Conver(this SecurityStaticInfo info)
        {
            return new SecurityMessage()
            {
                SecurityId =new SecurityId() { SecurityCode=info.Basic.Security.Code},
                SecurityType = ((SecurityType)info.Basic.SecType).Conver(),
                Name = info.Basic.Name,
                MinVolume=info.Basic.LotSize,
                VolumeStep = info.Basic.LotSize
            };
        }
    }
}
