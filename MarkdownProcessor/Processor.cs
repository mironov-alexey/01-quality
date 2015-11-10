using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarkdownProcessor
{
    public class Processor
    {
        private static readonly HashSet<string> TagsIgnoreInsideTags = new HashSet<string>()
        {
            @"`"
        };
        private readonly string _inputText;
        private static readonly Dictionary<string, string> DefaultTags = new Dictionary<string, string>()
        {
            {"__", "strong"},
            {"_", "em"},
            {"`", "code"}
        };
        private readonly Dictionary<string, string> _tags;

        public Processor(string inputText, Dictionary<string, string> tags=null)
        {
            _inputText = inputText;
            _tags = tags ?? DefaultTags;
        }

        public string ReplaceParagraphTags(string text)
        {
            return Regex.Replace(text, @"(\n\s*\n)|(\r\n\s*\r\n)", "\n</p>\n<p>\n    ");
        }

        public string ReplaceParagraphTags()
        {
            return ReplaceParagraphTags(_inputText);
        }
        public string RenderText()
        {
            var openingTags = GetOpeningTagCandidates().Where(IsOpeningTag).ToList();
            var closingTags = GetClosingTagCandidates().Where(IsClosingTag).ToList();
            if (openingTags.Count == 0 || closingTags.Count == 0)
                return _inputText;
            var result = _inputText.Substring(0, openingTags[0].Position);
            for (var i = 0; i < openingTags.Count; i++)
            {
                var openingTag = openingTags[i];
                var closingTag = GetPairTag(openingTag, closingTags);
                if (closingTag == null)
                {
                    result += GetHandledUnpairedText(openingTags, i, openingTag);
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
            return result;
        }


        private string GetHandledUnpairedText(List<Tag> openingTags, int openingTagIndex, Tag openingTag)
        {
            if (openingTagIndex + 1 < openingTags.Count)
                return _inputText.Substring(openingTag.Position, openingTags[openingTagIndex + 1].Position - openingTag.Position);
            return _inputText.Substring(openingTag.Position);
        }

        public static string RenderText(string inputText)
        {
            return new Processor(inputText).RenderText();
        }

        private string AddEndOfText(string result, Tag closingTag, IReadOnlyList<Tag> openingTags, int i)
        {
            return result + _inputText.Substring(closingTag.Position + closingTag.Value.Length,
                openingTags[i + 1].Position - (closingTag.Position + closingTag.Value.Length));
        }

        private string RenderTagAndContent(string text, Tag openingTag, Tag closingTag)
        {
            var currentSubstring = text.Substring(
                openingTag.Position + openingTag.Value.Length,
                closingTag.Position - openingTag.Position - openingTag.Value.Length);
            if (IsCodeTag(openingTag)) // заменить на Contains
                currentSubstring = currentSubstring
                    .Replace("\r\n\r\n", " ")
                    .Replace("\n\n", " ")
                    .Replace("\r\n", " ")
                    .Replace("\n", " ");
            else
            {
                currentSubstring = RenderText(currentSubstring);
                currentSubstring = currentSubstring
                    .Replace("\r\n\r\n", " ")
                    .Replace("\n\n", " ")
                    .Replace("\r\n", " ")
                    .Replace("\n", " ");
            }
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

        public IEnumerable<Match> GetAllCandidates(string text)
        {
            return Regex
                .Matches(text, @"(" + string.Join(@")|(", _tags.Keys.OrderByDescending(key => key.Length)) + @")")
                .Cast<Match>();
        } 
        public IEnumerable<Tag> GetOpeningTagCandidates()
        {
            return GetAllCandidates(_inputText)
                .Select(m => new Tag(m.Index,
                    m.Value,
                    TagType.Opening, 
                    TagsIgnoreInsideTags.Contains(m.Value)))
                .ToDictionary(tag => tag.Position, tag => tag)
                .Values
                .OrderBy(tag => tag.Position);
        }

        private IEnumerable<Tag> GetClosingTagCandidates()
        {
            return GetAllCandidates(string.Join("", _inputText.Reverse()))
                .Select(m => new Tag(
                    _inputText.Length - m.Index - m.Value.Length, 
                    m.Value, TagType.Closing,
                    TagsIgnoreInsideTags.Contains(m.Value)))
                .Reverse()
                .ToDictionary(tag => tag.Position, tag => tag)
                .Values
                .OrderBy(tag => tag.Position);
        }

        public bool IsPunctuationCharacterAtIndex(int charIndex, TagType type)
        {
            if (charIndex < 0 || charIndex > _inputText.Length - 1)
                return type == TagType.Opening;
            return _inputText[charIndex] != '\\' && char.IsPunctuation(_inputText[charIndex]);
        }

        public bool IsWhiteSpaceCharacterAtIndex(int charIndex)
        {
            if (charIndex < 0 || charIndex > _inputText.Length - 1)
                return true;
            var isSpace = char.IsWhiteSpace(_inputText[charIndex]);
            return isSpace;
        }
        public bool IsWordAtIndex(int charIndex)
        {
            if (charIndex < 0 || charIndex > _inputText.Length - 1)
                return false;
            var isLetter = char.IsLetter(_inputText[charIndex]);
            var isNotDigit = !char.IsDigit(_inputText[charIndex]);
            return isLetter && isNotDigit;
        }

        public bool IsOpeningTag(Tag tag)
        {
            var isEndOfText = tag.Position == _inputText.Length - 1;
            if (isEndOfText)
                return false;
            var rightIndex = tag.Position + tag.Value.Length;
            var leftIndex = tag.Position - 1;
            var isStartOfText = tag.Position == 0;
            return
                (IsWordAtIndex(rightIndex) ||
                 IsPunctuationCharacterAtIndex(rightIndex, tag.TagType)) &&
                (isStartOfText || IsWhiteSpaceCharacterAtIndex(leftIndex) || 
                IsPunctuationCharacterAtIndex(leftIndex, tag.TagType));
        }

        public bool IsClosingTag(Tag tag)
        {
            var isStartOfText = tag.Position == 0;
            if (isStartOfText)
                return false;
            var rightIndex = tag.Position + tag.Value.Length;
            var leftIndex = tag.Position - 1;
            var isEndOfText = _inputText.Length - 1 == tag.Position;
            return
                (IsWordAtIndex(leftIndex) || IsPunctuationCharacterAtIndex(leftIndex, tag.TagType)) &&
                (isEndOfText || IsWhiteSpaceCharacterAtIndex(rightIndex) ||
                 IsPunctuationCharacterAtIndex(rightIndex, tag.TagType));
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