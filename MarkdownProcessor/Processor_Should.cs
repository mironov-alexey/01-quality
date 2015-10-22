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

        private readonly string someText = "_a_";
        private Processor processor;
        [SetUp]
        public void SetUp()
        {
            processor = new Processor(someText);
        }

        [Test]
        public void Identify_CorrectOpeningTags()
        {
            var text = "_a";
            var processor = new Processor(text);
            Assert.IsTrue(processor.IsOpeningTag(0));
        }

        [Test]
        public void Identify_CorrectClosingTags()
        {
            var text = "a_";
            var processor = new Processor(text);
            Assert.IsTrue(processor.IsClosingTag(2));
        }

        [Test]
        public void NotIdentify_IncorrectOpeningTags()
        {
            var text = "_1";
            var processor = new Processor(text);
            Assert.IsFalse(processor.IsOpeningTag(0));
        }

        [Test]
        public void NotIdentify_IncorrectClosingTags()
        {
            var text = "1_";
            var processor = new Processor(text);
            Assert.IsFalse(processor.IsOpeningTag(1));
        }

    }
}
