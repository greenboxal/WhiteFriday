using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteFriday
{
    public class ProductDescriptor
    {
        public int ProductID { get; set; }
        public string ProductUrl { get; set; }
        public PriceData Price { get; set; }
    }
}
