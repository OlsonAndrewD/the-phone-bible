namespace Garnet.Domain.Entities
{
    public class BookGroup
    {
        public string Name { get; set; }
        public BookGroup Parent { get; set; }

        public static readonly BookGroup EntireBible = new BookGroup { Name = "The Bible" };

        public static readonly BookGroup OldTestament =
            new BookGroup { Parent = EntireBible, Name = "Old Testament" };

        public static readonly BookGroup NewTestament =
            new BookGroup { Parent = EntireBible, Name = "New Testament" };
    }
}
