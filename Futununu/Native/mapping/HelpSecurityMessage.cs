using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using static Futu.OpenApi.Pb.QotCommon;

namespace StockSharp.Futunn.Native.mapping
{
   public static class HelpSecurityMessage
    {
        public static SecurityMessage Convert(this SecurityStaticInfo info)
        {
            return new SecurityMessage()
            {
                SecurityId = new SecurityId() { SecurityCode = info.Basic.Security.Code, BoardCode = "FUTU" },
                SecurityType = ((SecurityType)info.Basic.SecType).Convert(),
                Name = info.Basic.Name,
                MinVolume = info.Basic.LotSize,
                VolumeStep = info.Basic.LotSize,
                //BackMode= MessageBackModes.Direct,
            };
        }
    }
}
