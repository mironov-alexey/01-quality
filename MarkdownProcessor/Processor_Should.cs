using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MarkdownProcessor
{
    [TestFixture]
    public class Processor_Should
    {
        private Processor processor;

        [TestCase("aa", Result = true)]
        [TestCase("00", Result = false)]
        [TestCase("..", Result = false)]
        [TestCase("__", Result = false)]
        [TestCase("--", Result = false)]
        [TestCase("яя", Result = true)]
        [TestCase("я", Result = false)]
        public bool Identify_WordCharacter(string text)
        {
            return new Processor(text).IsWordCharacter(1);
        }

        [TestCase("_ ", Result = true)]
        [TestCase("_.", Result = true)]
        [TestCase("_", Result = true)]
        [TestCase("_\n", Result = true)]
        [TestCase("_a", Result = false)]
        [TestCase("_1", Result = false)]
        [TestCase("_\t", Result = true)]
        [TestCase("_\r", Result = true)]
        public bool Identify_WhiteSpaceCharacter(string text)
        {
            return new Processor(text).IsWhiteSpaceOrPunctuationChar(1);
        }

        [TestCase(0, "_", Result = true)]
        [TestCase(2, "_", Result = false)]
        [TestCase(5, "__", Result = true)]
        [TestCase(5, "_", Result = false)]
        [TestCase(5, "__", Result = true)]
        [TestCase(8, "_", Result = false)]
        [TestCase(11, "`", Result = true)]
        [TestCase(13, "`", Result = false)]
        public bool Correctly_IdentifyOpeningTags(int position, string tag)
        {
            processor = new Processor("_a_b __c_ ,`d`");
            return processor.IsOpeningTag(new Tag(position, tag));
        }

        [TestCase(0, "_", Result = false)]
        [TestCase(2, "_", Result = false)]
        [TestCase(4, "_", Result = true)]
        [TestCase(7, "__", Result = true)]
        [TestCase(11, "`", Result = true)]
        public bool Correctly_IdentifyClosingTags(int position, string tag)
        {
            processor = new Processor("_a_b_ c__ d`.");
            return processor.IsClosingTag(new Tag(position, tag));
        }

        [TestCase("lalal", Result = "<p>lalal</p>")]
        public string NotChange_IfThereAreNotTags(string text)
        {
            return new Processor(text).RenderTags();
        }

        [TestCase("a\n\nb\r\n\r\nc", Result = "<p>a</p><p>b</p><p>c</p>")]
        public string Correctly_RenderParagraphTags(string text)
        {
            return new Processor(text).RenderTags();
        }

        [TestCase("_a_", Result = "<p><em>a</em></p>")]
        [TestCase("_a _", Result = "<p>_a _</p>")]
        [TestCase("_a c_", Result = "<p><em>a c</em></p>")]
        [TestCase("_a_ _c_", Result = "<p><em>a</em> <em>c</em></p>")]
        [TestCase("_a_ d__ _c_", Result = "<p><em>a</em> d__ <em>c</em></p>")]
        public string Correctly_RenderEmTags(string text)
        {
            return new Processor(text).RenderTags();
        }

        [TestCase("__a__", Result = "<p><strong>a</strong></p>")]
        [TestCase("__a __", Result = "<p>__a __</p>")]
        [TestCase("__a__ b__", Result = "<p><strong>a</strong> b__</p>")]
        [TestCase("__a b__", Result = "<p><strong>a b</strong></p>")]
        [TestCase("__a__ __b__", Result = "<p><strong>a</strong> <strong>b</strong></p>")]
        public string Correctly_RenderStrongTags(string text)
        {
            return new Processor(text).RenderTags();
        }

        [TestCase("`a`", Result = "<p><code>a</code></p>")]
        [TestCase("`a `", Result = "<p>`a `</p>")]
        [TestCase("`a` b`", Result = "<p><code>a</code> b`</p>")]
        [TestCase("`a b`", Result = "<p><code>a b</code></p>")]
        [TestCase("`a` `b`", Result = "<p><code>a</code> <code>b</code></p>")]
        public string Correctly_RenderCodeTags(string text)
        {
            return new Processor(text).RenderTags();
        }

        [TestCase("`a _b_ c`", Result = "<p><code>a _b_ c</code></p>")]
        [TestCase("`a __b__ c`", Result = "<p><code>a __b__ c</code></p>")]
        [TestCase("`a `b` c`", Result = "<p><code>a `b</code> c`</p>")]
        public string NotRender_TagsInCodeTag(string text)
        {
            return new Processor(text).RenderTags();
        }

        [TestCase("_a __b__ c_", Result = "<p><em>a <strong>b</strong> c</em></p>")]
        [TestCase("_a `b` c_", Result = "<p><em>a <code>b</code> c</em></p>")]
        [TestCase("__a _b_ c__", Result = "<p><strong>a <em>b</em> c</strong></p>")]
        [TestCase("__a `b` c__", Result = "<p><strong>a <code>b</code> c</strong></p>")]
        public string Render_OneTagInsideAnotherNotCode(string text)
        {
            return new Processor(text).RenderTags();
        }

        [TestCase("_a b__ c`", Result = "<p>_a b__ c`</p>")]
        [TestCase("_a b_ c`", Result = "<p><em>a b</em> c`</p>")]
        [TestCase("_a b` c_", Result = "<p><em>a b` c</em></p>")]
        public string NotRender_UnpairedTags(string text)
        {
            return new Processor(text).RenderTags();
        }

        [TestCase("_a_c_b_", Result = "<p><em>a_c_b</em></p>")]
        [TestCase("_a_1_2_c_", Result = "<p><em>a_1_2_c</em></p>")]
        public string NotRender_TagsInsideDigitsOrText(string text)
        {
            return new Processor(text).RenderTags();
        }
    }
}
