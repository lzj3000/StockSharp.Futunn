using System;
using StockSharp.Messages;
using static Futu.OpenApi.Pb.TrdCommon;

namespace StockSharp.Futunn.Native.mapping
{
    public static class HelpSides
    {
        public static Sides Convert(this TrdSide side)
        {
            //if (side == TrdSide.TrdSide_Buy || side == TrdSide.TrdSide_BuyBack)
            //    return Sides.Buy;
            if (side == TrdSide.TrdSide_Sell || side == TrdSide.TrdSide_SellShort)
                return Sides.Sell;
            return Sides.Buy;
        }
        public static TrdSide Convert(this Sides side) {
            if (side == Sides.Sell)
                return TrdSide.TrdSide_Sell;
            return TrdSide.TrdSide_Buy;
        }
    }
}
