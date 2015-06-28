using System;

namespace Garnet.Domain.Entities
{
    public class Chapter
    {
        public Book Book { get; set; }
        public int ChapterNumber { get; set; }

        public static Chapter Parse(string chapterString)
        {
            throw new NotImplementedException();
        }
    }
}
