using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MarkdownProcessor
{
    public class Processor
    {
        public string Replace(string textForRendering, string markdownTag, string htmlTag)
        {
            var replacer = new Regex(@"([^\w\\]|^)" + markdownTag + @"(.*?[^\\])" + markdownTag + @"(\W|$)");
            return replacer.Replace(textForRendering,
                match => match.Groups[1].Value +
                    "<" + htmlTag + ">" + match.Groups[2].Value + "</" + htmlTag + ">" +
                    match.Groups[3].Value);
        }

        public string Render(string textForRendering)
        {
            var htmlTags = new List<string>()
            {
                "strong",
                "em",
                "code"
            };
            var markdownTags = new List<string>()
            {
                "__",
                "_",
                "`"
            };
            var renderedText = textForRendering;
            for (var i = 0; i < htmlTags.Count; i++)
                renderedText = Replace(renderedText, markdownTags[i], htmlTags[i]);
            renderedText = renderedText.Replace(@"\_", "_");
            renderedText = renderedText.Replace(@"\`", "`");
            return renderedText;
        }
    }
}
