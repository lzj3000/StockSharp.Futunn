using System;
using Ecng.ComponentModel;

namespace StockSharp.Futunn.component
{
    public class FutunnIconAttribute : IconAttribute
    {
        public FutunnIconAttribute(string icon):
            base($"/StockSharp.Futunn;component/logos/{icon}", true)
        {
        }
    }
}
