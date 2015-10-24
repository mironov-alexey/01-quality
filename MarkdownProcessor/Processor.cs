using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MarkdownProcessor
{
    public class Processor
    {
        private Dictionary<string, string> Markdown2HtmlTags;

        public Processor()
        {
            Markdown2HtmlTags = new Dictionary<string, string>()
            {
                {"__", "strong"},
                {"_", "em"},
                {"`", "code"}
            };
        }
//        public string Replace(string textForRendering, string markdownTag, string htmlTag)
//        {
//            var replacer = new Regex(@"([^\w\\]|^)" + markdownTag + @"(.*?[^\\])" + markdownTag + @"(\W|$)");
//            return replacer.Replace(textForRendering,
//                match => match.Groups[1].Value +
//                    "<" + htmlTag + ">" + match.Groups[2].Value + "</" + htmlTag + ">" +
//                    match.Groups[3].Value);
//        }
        public bool IsOpenTag(string text, int position)
        {
            if (position < 0 || text.Length < position)
                return false;
            return ((position == 0 && Markdown2HtmlTags.ContainsKey(text[position].ToString())) ||
                    !Regex.IsMatch(text[position - 1].ToString(), @"[\d\\]"));
        }

        public bool IsParagraphTag(string text, int position)
        {
            if (position < 0 || text.Length - 1 < position)
                return false;
            return (text[position] == '\n' && text[position + 1] == '\n');
        }

        public string Render(string textForRendering)
        {
            
//            var renderedText = textForRendering;
//            foreach (var markdown2HtmlTag in Markdown2HtmlTags)
//                renderedText = Replace(renderedText, markdown2HtmlTag.Key, markdown2HtmlTag.Value);
//            renderedText = renderedText.Replace(@"\_", "_");
//            renderedText = renderedText.Replace(@"\`", "`");
//            return renderedText;
            return "";
        }
    }
}
