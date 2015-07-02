namespace Garnet.Domain.Entities
{
    public class Chapter
    {
        public Book Book { get; set; }
        public int ChapterNumber { get; set; }

        public override string ToString()
        {
            if (Book != null)
            {
                return string.Join(" ", Book.Name, ChapterNumber);
            }

            return base.ToString();
        }
    }
}
