using System;
using Ecng.ComponentModel;

namespace StockSharp.Futunn
{
    /// <summary>
    /// Icon
    /// </summary>
    public class FutunnIconAttribute : IconAttribute
    {
        /// <summary>
        /// init
        /// </summary>
        public FutunnIconAttribute():
            base("/StockSharp.Media;component/logos/futunn_logo.png", true)
        {
            //StockSharp.Futunn;component/logos/futunn_logo.png Unable to retrieve resource
        }
    }
}
