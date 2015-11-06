using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MarkdownProcessor
{
    class Program
    {
        private static void Main(string[] args)
        {
//            var input = "_a __b c_ `some \n\ncode is__ here`";
            var input = File.ReadAllText("in.txt");
            var res = new Processor(input).RenderTags();
            Console.WriteLine(res);
            File.WriteAllText("out.txt", res);
        }
    }
}
