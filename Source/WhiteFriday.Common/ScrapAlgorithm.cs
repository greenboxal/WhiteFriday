using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteFriday.Common
{
    public abstract class ScrapAlgorithm
    {
        public abstract PriceData ScrapPage(string pageText);
    }
}
