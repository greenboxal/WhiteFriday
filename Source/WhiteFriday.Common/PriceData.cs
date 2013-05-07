using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteFriday
{
    public class PriceData
    {
        public decimal FromPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal CurrentDiscount { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((PriceData)obj);
        }

        protected bool Equals(PriceData other)
        {
            return FromPrice == other.FromPrice && CurrentPrice == other.CurrentPrice && CurrentDiscount == other.CurrentDiscount;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = FromPrice.GetHashCode();
                hashCode = (hashCode * 397) ^ CurrentPrice.GetHashCode();
                hashCode = (hashCode * 397) ^ CurrentDiscount.GetHashCode();
                return hashCode;
            }
        }
    }
}
