namespace MarkdownProcessor
{
    public class Tag
    {
        public int Index{ get; }
        public string Value{ get; }
        public Tag(int index, string value)
        {
            Index = index;
            Value = value;
        }
    }
}