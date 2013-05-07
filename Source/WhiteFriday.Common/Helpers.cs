using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WhiteFriday
{
    public static class Helpers
    {
        public static string SelectNodeValue(this XmlNode node, string xpath, string defaultValue = null)
        {
            XmlNode n = node.SelectSingleNode(xpath);

            if (n == null)
                return defaultValue;

            return n.InnerText;
        }
    }
}
