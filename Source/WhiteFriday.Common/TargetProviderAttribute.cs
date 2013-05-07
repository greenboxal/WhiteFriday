using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteFriday
{
    public sealed class TargetProviderAttribute : Attribute
    {
        public string Name { get; set; }
        
        public TargetProviderAttribute(string name)
        {
            Name = name;
        }
    }
}
