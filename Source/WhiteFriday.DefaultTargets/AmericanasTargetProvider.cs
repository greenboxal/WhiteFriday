using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace WhiteFriday.DefaultTargets
{
    [TargetProvider("Americanas")]
    public class AmericanasTargetProvider : TargetProvider
    {
        private const string AnchorName = "prodBox";
        private const int FromPriceDepth = 8;
        private const int FromPriceOffset = 1;
        private const int CurrentPriceDepth = 9;
        private const int CurrentPriceOffset = 2;

        public AmericanasTargetProvider(IServiceController controller, XmlNode configuration)
            : base(controller, configuration)
        {
        }

        protected override PriceData Process(string pageText)
        {
            HtmlDocument doc = new HtmlDocument();
            PriceData data = new PriceData();

            try
            {
                doc.Load(new StringReader(pageText));
            }
            catch
            {
                // TODO: Log?
                return null;
            }

            XPathNavigator navigator = doc.CreateNavigator();

            if (navigator == null)
                return null;

            HtmlNodeNavigator anchor = (HtmlNodeNavigator)navigator.SelectSingleNode("//*[@class='" + AnchorName + "']");

            if (anchor == null)
                return null;

            data.FromPrice = decimal.Parse(GetDepthFirstValue(anchor, FromPriceDepth, FromPriceOffset).Value.Replace("De:","").Replace("R$",""), CultureInfo.GetCultureInfo("pt-BR"));
            data.CurrentPrice = decimal.Parse(GetDepthFirstValue(anchor, CurrentPriceDepth, CurrentPriceOffset).Value, CultureInfo.GetCultureInfo("pt-BR"));

            return data;
        }

        private HtmlNodeNavigator GetDepthFirstValue(HtmlNodeNavigator anchor, int depth, int offset)
        {
            string depthQuery = "*";

            for (int i = 0; i < depth; i++)
                depthQuery += "/*";

            foreach (HtmlNodeNavigator item in anchor.Select(depthQuery))
            {
                if (offset-- > 0)
                    continue;

                return item;
            }

            return null;
        }
    }    
}
