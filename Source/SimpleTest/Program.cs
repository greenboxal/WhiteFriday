using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteFriday.DefaultTargets;

namespace SimpleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SubmarinoScrapAlgorithm test = new SubmarinoScrapAlgorithm();
            test.ScrapPage(File.ReadAllText(@"D:\test.html"));
        }
    }
}
