namespace MarkdownProcessor
{
    public class Tag
    {
        public int Position{ get; }
        public string Value{ get; }
        public bool IgnoreInsideTags{ get; }
        public TagType TagType{ get; }
        public Tag(int position, string value, TagType type=TagType.Opening, bool ignoreInsideTags=false)
        {
            Position = position;
            Value = value;
            IgnoreInsideTags = ignoreInsideTags;
            TagType = type;
        }

        public bool IsOpening() => TagType == TagType.Opening;
    }

    public enum TagType
    {
        Opening,
        Closing
    }
}