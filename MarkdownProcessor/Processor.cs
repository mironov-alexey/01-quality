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
        private readonly bool _isSubtext;

        public Processor(string inputText, Dictionary<string, string> tags, bool isSubtext = false)
        {
            _inputText = inputText;
            _tags = tags;
            _isSubtext = isSubtext;
        }
        public Processor(string inputText, bool isSubtext = false)
        {
            _inputText = inputText;
            _isSubtext = isSubtext;
            _tags = new Dictionary<string, string>()
            {
                {"__", "strong" },
                {"_", "em"},
                {"`", "code" }
            };
        }
        public string ReplaceParagraphTags(string text)
        {
            text = "<p>" + text + "</p>";
            return text.Replace("\n\n", "</p><p>").Replace("\r\n\r\n", "</p><p>");
        }
        public string RenderTags()
        {
            var candidates = GetTagCandidates().ToList();
            var openingTags = candidates.Where(IsOpeningTag).ToList();
            var closingTags = candidates.Where(IsClosingTag).ToList();
            if (openingTags.Count == 0 || closingTags.Count == 0)
                return _isSubtext ? _inputText : ReplaceParagraphTags(_inputText);
            var result = _inputText.Substring(0, openingTags[0].Position);
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
                i = openingTags.FindIndex(tag => tag.Position > closingTag.Position) - 1;
                if (i < 0)
                {
                    result += _inputText.Substring(closingTag.Position + closingTag.Value.Length);
                    break;
                }
                result = AddEndOfText(result, closingTag, openingTags, i);
            }
            foreach (var key in _tags.Keys.OrderByDescending(k => k.Length))
                result = result.Replace(@"\" + key, key);
            return _isSubtext ? result : ReplaceParagraphTags(result);
        }

        private string GetHandledUnpairedText(List<Tag> openingTags, string result, int i, Tag openingTag)
        {
            if (i + 1 < openingTags.Count)
                result += _inputText.Substring(openingTag.Position, openingTags[i + 1].Position - openingTag.Position);
            else
                result += _inputText.Substring(openingTag.Position);
            return result;
        }

        public string RenderTags(string inputText)
        {
            return new Processor(inputText, isSubtext: true).RenderTags();
        }
        private string AddEndOfText(string result, Tag closingTag, IReadOnlyList<Tag> openingTags, int i)
        {
            result += _inputText.Substring(closingTag.Position + closingTag.Value.Length,
                openingTags[i + 1].Position - (closingTag.Position + closingTag.Value.Length));
            return result;
        }
        private string RenderTagAndContent(string text, Tag openingTag, Tag closingTag)
        {
            var currentSubstring = text.Substring(
                openingTag.Position + openingTag.Value.Length,
                closingTag.Position - openingTag.Position - openingTag.Value.Length);
            if (!IsCodeTag(openingTag))
                currentSubstring = RenderTags(currentSubstring);
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
                return closingTags.First(tag => tag.Position > openingTag.Position && tag.Value == openingTag.Value);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
        public IEnumerable<Tag> GetTagCandidates()
        {
            return Regex
                .Matches(_inputText, @"(" + string.Join(@")|(", _tags.Keys.OrderByDescending(key => key.Length)) + @")")
                .Cast<Match>().Select(m => new Tag(m.Index, m.Value));
        }
        public bool IsWhiteSpaceOrPunctuationChar(int charIndex)
        {
            if (charIndex < 0 || charIndex > _inputText.Length - 1)
                return true;
            var isSpace = char.IsWhiteSpace(_inputText[charIndex]);
            var isPunctuation = _inputText[charIndex] != '\\' && char.IsPunctuation(_inputText[charIndex]);
            return isSpace || isPunctuation;
        }
        public bool IsWordCharacter(int charIndex)
        {
            if (charIndex < 0 || charIndex > _inputText.Length - 1)
                return false;
            var isLetter = char.IsLetter(_inputText[charIndex]);
            var isNotDigit = !char.IsDigit(_inputText[charIndex]);
            return isLetter && isNotDigit;
        }
        public bool IsOpeningTag(Tag tag)
        {
            if (tag.Position == _inputText.Length - 1)
                return false;
            return
                tag.Position == 0 && IsWordCharacter(tag.Position + tag.Value.Length) ||
                IsWhiteSpaceOrPunctuationChar(tag.Position - 1) && IsWordCharacter(tag.Position + tag.Value.Length);
        }
        public bool IsClosingTag(Tag tag)
        {
            if (tag.Position == 0)
                return false;
            return tag.Position == (_inputText.Length - 1) && IsWordCharacter(tag.Position - 1) ||
                   IsWordCharacter(tag.Position - 1) && IsWhiteSpaceOrPunctuationChar(tag.Position + tag.Value.Length);
        }
    }

    public class Tag
    {
        public int Position{ get; }
        public string Value{ get; }
        public Tag(int position, string value)
        {
            Position = position;
            Value = value;
        }
    }
}
