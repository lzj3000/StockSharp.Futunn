using StockSharp.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using static Futu.OpenApi.Pb.QotCommon;

namespace StockSharp.Futunn.Native.mapping
{
    public static class HelpSecurityTypes
    {
        public static SecurityType Conver(this SecurityTypes securityTypes) {
            return outConv(securityTypes);
        }
        public static SecurityTypes Conver(this SecurityType securityType) {
            return inConv(securityType);
        }

        private static SecurityType outConv(SecurityTypes securityTypes) {
            switch (securityTypes)
            {
                case SecurityTypes.Stock:
                    return SecurityType.SecurityType_Eqty;
                case SecurityTypes.Future:
                    return SecurityType.SecurityType_Future;
                case SecurityTypes.Option:
                    return SecurityType.SecurityType_Drvt;
                case SecurityTypes.Index:
                    return SecurityType.SecurityType_Index;
                case SecurityTypes.Warrant:
                    return SecurityType.SecurityType_Warrant;
                default:
                    return SecurityType.SecurityType_Unknown;
            }
        }
        private static SecurityTypes inConv(SecurityType securityType)
        {
            switch (securityType)
            {
                case SecurityType.SecurityType_Eqty:
                    return SecurityTypes.Stock;
                case SecurityType.SecurityType_Future:
                    return SecurityTypes.Future;
                case SecurityType.SecurityType_Drvt:
                    return SecurityTypes.Option;
                case SecurityType.SecurityType_Index:
                    return SecurityTypes.Index;
                case SecurityType.SecurityType_Warrant:
                    return SecurityTypes.Warrant;
                default:
                    return SecurityTypes.Commodity;
            }
        }
    }
}