using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownProcessor
{
    class Program
    {
        private static void Main(string[] args)
        {
            var d = new Dictionary<string, string>()
            {
                {"a", "1"},
                {"b", "2"},
                {"c", "3"},
            };
            foreach (var item in d)
                Console.WriteLine("{0} {1}", item.Key, item.Value);
        }
    }
}
