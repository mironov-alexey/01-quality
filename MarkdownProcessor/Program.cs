using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace MarkdownProcessor
{
    class Program
    {
        private static void Main(string[] args)
        {
            var input = File.ReadAllText("in.txt");
//            var res = new Processor(input).RenderText();
            var res = new Processor(input).GetHtmlCode();
            Console.WriteLine(res);
            File.WriteAllText("out.txt", res);
        }
    }
}
