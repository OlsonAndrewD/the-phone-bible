using System.Linq;
using System.Collections.Generic;
using Garnet.Domain.Entities;
using System.Threading.Tasks;

namespace Garnet.Services
{
    public abstract class BibleContentService
    {
        public Chapter GetDefaultChapter()
        {
            return new Chapter
            {
                BookName = "John",
                ChapterNumber = 1
            };
        }

        public Chapter GetChapterAfter(Chapter chapter)
        {
            var book = GetBook(chapter.BookName);
            if (chapter.ChapterNumber < book.NumberOfChapters)
            {
                return new Chapter
                {
                    BookName = book.Name,
                    ChapterNumber = chapter.ChapterNumber + 1
                };
            }

            var allBooks = GetBooks();
            var nextBook = allBooks.SkipWhile(x => x.Name != book.Name).Skip(1).FirstOrDefault();
            if (nextBook == null)
            {
                nextBook = allBooks.First();
            }

            return new Chapter
            {
                BookName = nextBook.Name,
                ChapterNumber = 1
            };
        }

        public Book GetBook(string bookName)
        {
            return Books.FirstOrDefault(x => x.Name == bookName);
        }

        public BookGroup GetGroup(string groupName = null)
        {
            groupName = groupName ?? EntireBible.Name;
            return BookGroups.FirstOrDefault(x => x.Name == groupName);
        }

        public IEnumerable<BookGroup> GetChildGroups(string parentGroupName)
        {
            parentGroupName = parentGroupName ?? EntireBible.Name;
            return BookGroups.Where(x => x.Parent != null && x.Parent.Name == parentGroupName);
        }

        public IEnumerable<Book> GetBooks(string groupName = null)
        {
            if (groupName == null)
            {
                return Books;
            }
            else
            {
                return Books.Where(x => x.Group.Name == groupName);
            }
        }

        protected static readonly BookGroup EntireBible = new BookGroup { Name = "Entire Bible" };

        protected static readonly BookGroup OldTestament = 
            new BookGroup { Parent = EntireBible, Name = "Old Testament" };
        private static readonly BookGroup BooksOfMoses =
            new BookGroup { Parent = OldTestament, Name = "Books of Moses" };
        private static readonly BookGroup HistoricalBooks = 
            new BookGroup { Parent = OldTestament, Name = "Historical Books" };
        private static readonly BookGroup PoetryAndWisdom = 
            new BookGroup { Parent = OldTestament, Name = "Poetry and Wisdom" };
        private static readonly BookGroup MajorProphets = 
            new BookGroup { Parent = OldTestament, Name = "Major Prophets" };
        private static readonly BookGroup MinorProphets = 
            new BookGroup { Parent = OldTestament, Name = "Minor Prophets" };

        protected static readonly BookGroup NewTestament =
            new BookGroup { Parent = EntireBible, Name = "New Testament" };
        private static readonly BookGroup GospelsAndActs = 
            new BookGroup { Parent = NewTestament, Name = "Gospels and Acts" };
        private static readonly BookGroup PaulsLetters = 
            new BookGroup { Parent = NewTestament, Name = "Paul's Letters" };
        private static readonly BookGroup GeneralLettersAndProphecy = 
            new BookGroup { Parent = NewTestament, Name = "General Letters and Prophesy" };

        private static readonly IEnumerable<BookGroup> BookGroups = new[]
        {
            EntireBible,

            OldTestament,
            BooksOfMoses,
            HistoricalBooks,
            PoetryAndWisdom,
            MajorProphets,
            MinorProphets,

            NewTestament,
            GospelsAndActs,
            PaulsLetters,
            GeneralLettersAndProphecy
        };

        private static readonly IEnumerable<Book> Books = new[]
        {
            new Book
            {
                Name = "Genesis",
                NumberOfChapters = 50,
                Group = BooksOfMoses,
            },
            new Book
            {
                Name = "Exodus",
                NumberOfChapters = 40,
                Group = BooksOfMoses,
            },
            new Book
            {
                Name = "First Chronicles",
                NumberOfChapters = 29,
                Group = HistoricalBooks,
            },
            new Book
            {
                Name = "Second Chronicles",
                NumberOfChapters = 36,
                Group = HistoricalBooks,
            },
            new Book
            {
                Name = "Psalms",
                NumberOfChapters = 150,
                Group = PoetryAndWisdom,
            },
            new Book
            {
                Name = "Proverbs",
                NumberOfChapters = 31,
                Group = PoetryAndWisdom,
            },
            new Book
            {
                Name = "Matthew",
                NumberOfChapters = 28,
                Group = GospelsAndActs,
            },
            new Book
            {
                Name = "Mark",
                NumberOfChapters = 16,
                Group = GospelsAndActs,
            },
        };
    }
}
