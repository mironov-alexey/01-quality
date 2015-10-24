using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MarkdownProcessor
{
    [TestFixture]
    public class Processor_Should
    {
        private readonly Processor processor = new Processor();
        [Test]
        public void NotChange_IfNoMarkup()
        {
            var input = "some string";
            Assert.AreEqual(input, processor.Render(input));
        }

        [TestCase("_a_", Result = "<em>a</em>")]
        public string CorrectlyRender_EmTag(string input)
        {
            return processor.Replace(input, "_", "em");
        }

        [TestCase("__a__", Result = "<strong>a</strong>")]
        [TestCase("_a __lol__", Result = "_a <strong>lol</strong>")]
        public string CorrectlyRender_StrongTag(string input)
        {
            return processor.Replace(input, "__", "strong");
        }

        [TestCase("`var a = 0;`", Result = "<code>var a = 0;</code>")]
        [TestCase("`a;` b", Result = "<code>a;</code> b")]
        public string CorrectlyRender_CodeTag(string input)
        {
            return processor.Replace(input, "`", "code");
        }

        [TestCase("_some __text__ is here_", Result = "<em>some <strong>text</strong> is here</em>")]
        [TestCase("_it __is__ it!_", Result = "<em>it <strong>is</strong> it!</em>")]
        [TestCase("_i __t __is__ it!_", Result = "<em>i <strong>t __is</strong> it!</em>")]
        [TestCase("`some __code__ is` here", Result = "<code>some <strong>code</strong> is</code> here")]
        [TestCase("`some _code_ is` here", Result = "<code>some <em>code</em> is</code> here")]
        [TestCase("_some `code` is_ here", Result = "<em>some <code>code</code> is</em> here")]
        public string CorrectlyRender_OneTagInsideAnother(string input)
        {
            return processor.Render(input);
        }

        [TestCase("_a", Result = "_a")]
        [TestCase("__a", Result = "__a")]
        [TestCase("a_", Result = "a_")]
        [TestCase("a__", Result = "a__")]
        [TestCase("a` ", Result = "a` ")]
        [TestCase(" `a", Result = " `a")]
        public string NotRender_TagWithoutPair(string input)
        {
            return processor.Render(input);
        }

        [TestCase(@"\_a_", Result = "_a_")]
        [TestCase(@"\_\_a__", Result = "__a__")]
        [TestCase(@"_\_\_a__", Result = "<em>__a_</em>")]
        [TestCase(@"\`a`", Result = "`a`")]
        public string NotRender_ShieldedMarks(string input)
        {
            return processor.Render(input);
        }
    }
}
