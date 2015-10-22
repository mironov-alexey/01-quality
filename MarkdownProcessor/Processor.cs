using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkdownProcessor
{
    public class Processor
    {
        private readonly string _text;
        private Dictionary<string, string> _markdown2openTags; 
        private Dictionary<string, string> _markdown2closeTags;
        public Processor(string text)
        {
            _text = text;
            _markdown2openTags = new Dictionary<string, string>()
            {
                {"_",  "<em>"},
                {"__",  "<strong>"},
                {"`",  "<code>"}
            };
            _markdown2closeTags = new Dictionary<string, string>()
            {
                {"_", "</em>" },
                {"__", "</strong>" },
                {"`", "</code>" }
            };
        }

        public bool IsOpeningTag(int index)
        {
            return Regex.IsMatch(_text.Substring(index + 1, 1), @"\w");
            throw new NotImplementedException();
        }

        public bool IsClosingTag(int index)
        {
            return Regex.IsMatch(_text.Substring(index - 1, 1), @"\w");
            throw new NotImplementedException();
        }

        private string GetHtmlView()
        {
            throw new NotImplementedException();
        }


    }
}
