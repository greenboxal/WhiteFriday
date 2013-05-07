using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WhiteFriday
{
    public interface IServiceController
    {
        XmlNode Configuration { get; }
        TimeSpan QueryInterval { get; }
        TimeSpan QueryDelay { get; }
        int MaxRetries { get; }

        void UpdateProductPrice(int targetID, int productID, PriceData data);
        ProductDescriptor[] QueryProducts(int targetID);
        void Stop();
    }
}
