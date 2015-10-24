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
            
        }

        [TestCase(new object[] {"_a", 0}, Result = true)]
        [TestCase(new object[] {" _a", 1}, Result = true)]
        [TestCase(new object[] {"a_a", 1}, Result = false)]
        [TestCase(new object[] {"1_a", 1}, Result = false)]
        [TestCase(new object[] {"`_a", 1}, Result = false)]
        [TestCase(new object[] {"__a", 1}, Result = false)]
        public bool CorrectlyIdentify_OpenTags(string text, int position)
        {
            return processor.IsOpenTag(text, position);
        }
    }
}
