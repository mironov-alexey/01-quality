using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarkdownProcessor
{
    public class Processor
    {
        private readonly string _inputText;
        private readonly Dictionary<string, string> _tags;

        public Processor(string inputText, Dictionary<string, string> tags)
        {
            _inputText = inputText;
            _tags = tags;
        }

        public Processor(string inputText)
        {
            _inputText = inputText;
            _tags = new Dictionary<string, string>()
            {
                {"__", "strong"},
                {"_", "em"},
                {"`", "code"}
            };
        }

        public string ReplaceParagraphTags(string text)
        {
            return Regex.Replace(text, @"(\n\s*\n)|(\r\n\s*\r\n)", "\n</p>\n<p>\n    ");
        }

        public string RenderText()
        {
            var openingTagsCandidates = GetOpeningTagCandidates().ToList();
            var closingTagsCandidates = GetClosingTagCandidates().ToList();
            var openingTags = openingTagsCandidates.Where(IsOpeningTag).ToList();
            var closingTags = closingTagsCandidates.Where(IsClosingTag).ToList();
            if (openingTags.Count == 0 || closingTags.Count == 0)
//                return _isSubtext ? _inputText : ReplaceParagraphTags(_inputText);
                return _inputText;
            var result = _inputText.Substring(0, openingTags[0].Index);
            for (var i = 0; i < openingTags.Count; i++)
            {
                var openingTag = openingTags[i];
                var closingTag = GetPairTag(openingTag, closingTags);
                if (closingTag == null)
                {
                    result = GetHandledUnpairedText(openingTags, result, i, openingTag);
                    continue;
                }
                result += RenderTagAndContent(_inputText, openingTag, closingTag);
                i = openingTags.FindIndex(tag => tag.Index > closingTag.Index) - 1;
                if (i < 0)
                {
                    result += _inputText.Substring(closingTag.Index + closingTag.Value.Length);
                    break;
                }
                result = AddEndOfText(result, closingTag, openingTags, i);
            }
            foreach (var key in _tags.Keys.OrderByDescending(k => k.Length))
                result = result.Replace(@"\" + key, key);
//            return _isSubtext ? result : ReplaceParagraphTags(result);
            return result;
        }


        private string GetHandledUnpairedText(List<Tag> openingTags, string result, int i, Tag openingTag)
        {
            if (i + 1 < openingTags.Count)
                result += _inputText.Substring(openingTag.Index, openingTags[i + 1].Index - openingTag.Index);
            else
                result += _inputText.Substring(openingTag.Index);
            return result;
        }

        public string RenderText(string inputText)
        {
            return new Processor(inputText).RenderText();
        }

        private string AddEndOfText(string result, Tag closingTag, IReadOnlyList<Tag> openingTags, int i)
        {
            result += _inputText.Substring(closingTag.Index + closingTag.Value.Length,
                openingTags[i + 1].Index - (closingTag.Index + closingTag.Value.Length));
            return result;
        }

        private string RenderTagAndContent(string text, Tag openingTag, Tag closingTag)
        {
            var currentSubstring = text.Substring(
                openingTag.Index + openingTag.Value.Length,
                closingTag.Index - openingTag.Index - openingTag.Value.Length);
            if (IsCodeTag(openingTag))
                currentSubstring = currentSubstring
                    .Replace("\r\n\r\n", " ")
                    .Replace("\n\n", " ")
                    .Replace("\r\n", " ")
                    .Replace("\n", " ");
            else
                currentSubstring = RenderText(currentSubstring);
            return "<" + _tags[openingTag.Value] + ">"
                   + currentSubstring + "</" + _tags[openingTag.Value] + ">";
        }

        public bool IsCodeTag(Tag tag)
        {
            return tag.Value == "`";
        }

        public Tag GetPairTag(Tag openingTag, IEnumerable<Tag> closingTags)
        {
            try
            {
                return closingTags.First(tag => tag.Index > openingTag.Index && tag.Value == openingTag.Value);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public IEnumerable<Tag> GetOpeningTagCandidates()
        {
            return Regex
                .Matches(_inputText, @"(" + string.Join(@")|(", _tags.Keys.OrderByDescending(key => key.Length)) + @")")
                .Cast<Match>().Select(m => new Tag(m.Index, m.Value))
                .ToDictionary(tag => tag.Index, tag => tag)
                .Values
                .OrderBy(tag => tag.Index);
        }

        private IEnumerable<Tag> GetClosingTagCandidates()
        {
            return Regex.Matches(string.Join("", _inputText.Reverse()),
                @"(" + string.Join(@")|(", _tags.Keys.OrderByDescending(key => key.Length)) + @")")
                .Cast<Match>()
                .Select(m => new Tag(_inputText.Length - m.Index - m.Value.Length, m.Value))
                .Reverse()
                .ToDictionary(tag => tag.Index, tag => tag)
                .Values
                .OrderBy(tag => tag.Index);
        }

        public bool IsWhiteSpaceOrPunctuationChar(int charIndex)
        {
            if (charIndex < 0 || charIndex > _inputText.Length - 1)
                return true;
            var isSpace = char.IsWhiteSpace(_inputText[charIndex]);
            var isPunctuation = _inputText[charIndex] != '\\' && char.IsPunctuation(_inputText[charIndex]);
            return isSpace || isPunctuation;
        }

        public bool IsWordOrPunctuationCharacter(int charIndex)
        {
            if (charIndex < 0 || charIndex > _inputText.Length - 1)
                return false;
            var isLetter = char.IsLetter(_inputText[charIndex]);
            var isNotDigit = !char.IsDigit(_inputText[charIndex]);
            var isPunctuation = _inputText[charIndex] != '\\' && char.IsPunctuation(_inputText[charIndex]);
            return (isLetter || isPunctuation) && isNotDigit;
        }

        public bool IsOpeningTag(Tag tag)
        {
            if (tag.Index == _inputText.Length - 1)
                return false;
            return
                tag.Index == 0 && IsWordOrPunctuationCharacter(tag.Index + tag.Value.Length) ||
                IsWhiteSpaceOrPunctuationChar(tag.Index - 1) &&
                IsWordOrPunctuationCharacter(tag.Index + tag.Value.Length);
        }

        public bool IsClosingTag(Tag tag)
        {
            if (tag.Index == 0)
                return false;
            return tag.Index == (_inputText.Length - 1) && IsWordOrPunctuationCharacter(tag.Index - 1) ||
                   IsWordOrPunctuationCharacter(tag.Index - 1) &&
                   IsWhiteSpaceOrPunctuationChar(tag.Index + tag.Value.Length);
        }

        public string GetHtmlCode()
        {
            return "<!DOCTYPE html>\n" +
                   "<html>\n" +
                   "<head>\n" +
                   "    <meta charset='utf-8'>\n" +
                   "</head>\n" +
                   "<body>\n" +
                   "<p>\n    " +
                   ReplaceParagraphTags(RenderText()) +
                   "\n</p>\n" +
                   "</body>\n" +
                   "</html>";
        }
    }
}