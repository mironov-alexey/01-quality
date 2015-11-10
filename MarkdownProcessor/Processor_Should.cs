using NUnit.Framework;

namespace MarkdownProcessor
{
    [TestFixture]
    public class Processor_Should
    {
        private Processor _processor;

        [TestCase("aa", Result = true)]
        [TestCase("00", Result = false)]
        [TestCase("яя", Result = true)]
        [TestCase("я", Result = false)]
        public bool Identify_WordCharacter(string text)
        {
            return new Processor(text).IsWordAtIndex(1);
        }

        [TestCase("_ ", Result = true)]
        [TestCase("_", Result = true)]
        [TestCase("_\n", Result = true)]
        [TestCase("_a", Result = false)]
        [TestCase("_1", Result = false)]
        [TestCase("_\t", Result = true)]
        [TestCase("_\r", Result = true)]
        public bool Identify_WhiteSpaceCharacter(string text)
        {
            return new Processor(text).IsWhiteSpaceCharacterAtIndex(1);
        }

        [TestCase("_.", Result = true)]
        [TestCase("..", Result = true)]
        [TestCase("__", Result = true)]
        [TestCase("--", Result = true)]
        public bool Identify_PunctuationCharacter(string text)
        {
            return new Processor(text).IsPunctuationCharacterAtIndex(1, TagType.Closing);
        }

        [TestCase(0, "_", Result = true)]
        [TestCase(2, "_", Result = false)]
        [TestCase(5, "__", Result = true)]
        [TestCase(5, "_", Result = true)]
        [TestCase(8, "_", Result = false)]
        [TestCase(11, "`", Result = true)]
        [TestCase(13, "`", Result = false)]
        public bool Correctly_IdentifyOpeningTags(int position, string tag)
        {
            _processor = new Processor("_a_b __c_ ,`d`");
            return _processor.IsOpeningTag(new Tag(position, tag));
        }

        [TestCase(0, "_", Result = false)]
        [TestCase(2, "_", Result = false)]
        [TestCase(4, "_", Result = true)]
        [TestCase(7, "__", Result = true)]
        [TestCase(11, "`", Result = true)]
        public bool Correctly_IdentifyClosingTags(int position, string tag)
        {
            _processor = new Processor("_a_b_ c__ d`.");
            return _processor.IsClosingTag(new Tag(position, tag, TagType.Closing));
        }

        [TestCase("lalal", Result = "lalal")]
        public string NotChange_IfThereAreNotTags(string text)
        {
            return new Processor(text).RenderText();
        }

        [TestCase("a\n\nb\r\n\r\nc", Result = "a\n</p>\n<p>\n    b\n</p>\n<p>\n    c")]
        public string Correctly_RenderParagraphTags(string text)
        {
            return new Processor(text).ReplaceParagraphTags();
        }

        [TestCase("_a_", Result = "<em>a</em>")]
        [TestCase("_a _", Result = "_a _")]
        [TestCase("_a c_", Result = "<em>a c</em>")]
        [TestCase("_a_ _c_", Result = "<em>a</em> <em>c</em>")]
        [TestCase("_a_ d__ _c_", Result = "<em>a</em> d__ <em>c</em>")]
        public string Correctly_RenderEmTags(string text)
        {
            return new Processor(text).RenderText();
        }

        [TestCase("__a__", Result = "<strong>a</strong>")]
        [TestCase("__a __", Result = "__a __")]
        [TestCase("__a__ b__", Result = "<strong>a</strong> b__")]
        [TestCase("__a b__", Result = "<strong>a b</strong>")]
        [TestCase("__a__ __b__", Result = "<strong>a</strong> <strong>b</strong>")]
        public string Correctly_RenderStrongTags(string text)
        {
            return new Processor(text).RenderText();
        }

        [TestCase("`a`", Result = "<code>a</code>")]
        [TestCase("`a `", Result = "`a `")]
        [TestCase("`a` b`", Result = "<code>a</code> b`")]
        [TestCase("`a b`", Result = "<code>a b</code>")]
        [TestCase("`a` `b`", Result = "<code>a</code> <code>b</code>")]
        public string Correctly_RenderCodeTags(string text)
        {
            return new Processor(text).RenderText();
        }

        [TestCase("`a _b_ c`", Result = "<code>a _b_ c</code>")]
        [TestCase("`a __b__ c`", Result = "<code>a __b__ c</code>")]
        [TestCase("`a `b` c`", Result = "<code>a `b</code> c`")]
        [TestCase("`a\n\nb`", Result = "<code>a b</code>")]
        public string NotRender_TagsInCodeTag(string text)
        {
            return new Processor(text).RenderText();
        }

        [TestCase("_a __b__ c_", Result = "<em>a <strong>b</strong> c</em>")]
        [TestCase("_a `b` c_", Result = "<em>a <code>b</code> c</em>")]
        [TestCase("__a _b_ c__", Result = "<strong>a <em>b</em> c</strong>")]
        [TestCase("__a `b` c__", Result = "<strong>a <code>b</code> c</strong>")]
        public string Render_OneTagInsideAnotherNotCode(string text)
        {
            return new Processor(text).RenderText();
        }

        [TestCase("_a b__ c`", Result = "_a b__ c`")]
        [TestCase("_a b_ c`", Result = "<em>a b</em> c`")]
        [TestCase("_a b` c_", Result = "<em>a b` c</em>")]
        [TestCase("___a___", Result = "<strong><em>a</em></strong>")]
        public string NotRender_UnpairedTags(string text)
        {
            return new Processor(text).RenderText();
        }

        [TestCase("_a_c_b_", Result = "<em>a_c_b</em>")]
        [TestCase("_a_1_2_c_", Result = "<em>a_1_2_c</em>")]
        public string NotRender_TagsInsideDigitsOrText(string text)
        {
            return new Processor(text).RenderText();
        }

        [TestCase("_a_ __b__ _c_", Result = "<em>a</em> <strong>b</strong> <em>c</em>")]
        [TestCase("_a __b__ _c_", Result = "<em>a <strong>b</strong> _c</em>")]
        [TestCase("1_a __b__ _c_", Result = "1_a <strong>b</strong> <em>c</em>")]
        [TestCase("__a__ _b_ __c__", Result = "<strong>a</strong> <em>b</em> <strong>c</strong>")]
        public string Render_SeveralTags(string text)
        {
            return new Processor(text).RenderText();
        }

    }
}
