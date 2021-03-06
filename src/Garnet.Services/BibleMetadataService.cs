﻿using Garnet.Domain.Entities;
using Garnet.Domain.Services;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Garnet.Services
{
    public class BibleMetadataService : IBibleMetadataService
    {
        public Chapter GetDefaultChapter()
        {
            return new Chapter
            {
                Book = GetBook("John"),
                ChapterNumber = 1
            };
        }

        public Chapter GetChapterAfter(Chapter chapter)
        {
            var book = chapter.Book;
            if (chapter.ChapterNumber < book.NumberOfChapters)
            {
                return new Chapter
                {
                    Book = book,
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
                Book = nextBook,
                ChapterNumber = 1
            };
        }

        public Book GetBook(string bookName)
        {
            return Books.Value.FirstOrDefault(x => x.Name == bookName);
        }

        public BookGroup GetGroup(string groupName = null)
        {
            groupName = groupName ?? BookGroup.EntireBible.Name;
            return BookGroups.Value.FirstOrDefault(x => x.Name == groupName);
        }

        public IEnumerable<BookGroup> GetChildGroups(string parentGroupName)
        {
            parentGroupName = parentGroupName ?? BookGroup.EntireBible.Name;
            return BookGroups.Value.Where(x => x.Parent != null && x.Parent.Name == parentGroupName);
        }

        public IEnumerable<Book> GetBooks(string groupName = null)
        {
            if (groupName == null)
            {
                return Books.Value;
            }
            else
            {
                return Books.Value.Where(x => x.Group.Name == groupName);
            }
        }

        public int GetChapterNumber(Chapter chapter)
        {
            var firstChapterNumberInBook = _firstChapterNumberLookup.Value[chapter.Book];
            return firstChapterNumberInBook + chapter.ChapterNumber - 1;
        }

        private static Lazy<Dictionary<Book, int>> _firstChapterNumberLookup = new Lazy<Dictionary<Book, int>>(() =>
        {
            var lookup = new Dictionary<Book, int>();
            var chapterNumber = 1;

            foreach (var book in Books.Value)
            {
                lookup.Add(book, chapterNumber);
                chapterNumber += book.NumberOfChapters;
            }

            return lookup;
        });

        public Chapter GetChapterByNumber(int chapterNumber)
        {
            var book = BinarySearchForBookThatContains(chapterNumber, 0, _bookLookupByChapterNumber.Value.Count - 1);
            if (book == null)
            {
                return null;
            }

            var firstChapterNumberInBook = _firstChapterNumberLookup.Value[book];
            return new Chapter
            {
                Book = book,
                ChapterNumber = chapterNumber - firstChapterNumberInBook + 1
            };
        }

        private Book BinarySearchForBookThatContains(int chapterNumber, int minIndex, int maxIndex)
        {
            if (minIndex > maxIndex)
            {
                return null;
            }

            var midIndex = minIndex + ((maxIndex - minIndex) / 2);
            var middleEntry = _bookLookupByChapterNumber.Value[midIndex];
            var middleBook = middleEntry.Value;
            var middleBookFirstChapterNumber = middleEntry.Key;
            var middleBookLastChapterNumber = middleBookFirstChapterNumber + middleBook.NumberOfChapters - 1;

            if (chapterNumber >= middleBookFirstChapterNumber &&
                chapterNumber <= middleBookLastChapterNumber)
            {
                return middleBook;
            }

            if (chapterNumber < middleBookFirstChapterNumber) {
                return BinarySearchForBookThatContains(chapterNumber, minIndex, midIndex - 1);
            }

            return BinarySearchForBookThatContains(chapterNumber, midIndex + 1, maxIndex);
        }

        private static Lazy<IList<KeyValuePair<int, Book>>> _bookLookupByChapterNumber = new Lazy<IList<KeyValuePair<int, Book>>>(() =>
        {
            var list = new List<KeyValuePair<int, Book>>();
            var chapterNumber = 1;

            foreach (var book in Books.Value)
            {
                list.Add(new KeyValuePair<int, Book>(chapterNumber, book));
                chapterNumber += book.NumberOfChapters;
            }

            return list;
        });

        private static readonly BookGroup BooksOfMoses =
            new BookGroup { Parent = BookGroup.OldTestament, Name = "Books of Moses" };
        private static readonly BookGroup HistoricalBooks =
            new BookGroup { Parent = BookGroup.OldTestament, Name = "Historical Books" };
        private static readonly BookGroup PoetryAndWisdom =
            new BookGroup { Parent = BookGroup.OldTestament, Name = "Poetry and Wisdom" };
        private static readonly BookGroup MajorProphets =
            new BookGroup { Parent = BookGroup.OldTestament, Name = "Major Prophets" };
        private static readonly BookGroup MinorProphets =
            new BookGroup { Parent = BookGroup.OldTestament, Name = "Minor Prophets" };

        private static readonly BookGroup GospelsAndActs =
            new BookGroup { Parent = BookGroup.NewTestament, Name = "Gospels and Acts" };
        private static readonly BookGroup PaulsLetters =
            new BookGroup { Parent = BookGroup.NewTestament, Name = "Paul's Letters" };
        private static readonly BookGroup GeneralLetters =
            new BookGroup { Parent = BookGroup.NewTestament, Name = "General Letters" };
        private static readonly BookGroup Apocalyptic =
            new BookGroup { Parent = BookGroup.NewTestament, Name = "Apocalyptic" };

        private static Lazy<IEnumerable<BookGroup>> BookGroups = new Lazy<IEnumerable<BookGroup>>(() => new[]
        {
            BookGroup.EntireBible,

            BookGroup.OldTestament,
            BooksOfMoses,
            HistoricalBooks,
            PoetryAndWisdom,
            MajorProphets,
            MinorProphets,

            BookGroup.NewTestament,
            GospelsAndActs,
            PaulsLetters,
            GeneralLetters,
            Apocalyptic
        });

        private static Lazy<IEnumerable<Book>> Books = new Lazy<IEnumerable<Book>>(() => new[]
        {
            new Book
            {
                DbpId = "Gen",
                Name = "Genesis",
                NumberOfChapters = 50,
                Group = BooksOfMoses,
            },
            new Book
            {
                DbpId = "Exod",
                Name = "Exodus",
                NumberOfChapters = 40,
                Group = BooksOfMoses,
            },
            new Book
            {
                DbpId = "Lev",
                Name = "Leviticus",
                NumberOfChapters = 27,
                Group = BooksOfMoses,
            },
            new Book
            {
                DbpId = "Num",
                Name = "Numbers",
                NumberOfChapters = 36,
                Group = BooksOfMoses,
            },
            new Book
            {
                DbpId = "Deut",
                Name = "Deuteronomy",
                NumberOfChapters = 34,
                Group = BooksOfMoses,
            },
            new Book
            {
                DbpId = "Josh",
                Name = "Joshua",
                NumberOfChapters = 24,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "Judg",
                Name = "Judges",
                NumberOfChapters = 21,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "Ruth",
                Name = "Ruth",
                NumberOfChapters = 4,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "1Sam",
                Name = "First Samuel",
                NumberOfChapters = 31,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "2Sam",
                Name = "Second Samuel",
                NumberOfChapters = 24,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "1Kgs",
                Name = "First Kings",
                NumberOfChapters = 22,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "2Kgs",
                Name = "Second Kings",
                NumberOfChapters = 25,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "1Chr",
                Name = "First Chronicles",
                NumberOfChapters = 29,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "2Chr",
                Name = "Second Chronicles",
                NumberOfChapters = 36,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "Ezra",
                Name = "Ezra",
                NumberOfChapters = 10,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "Neh",
                Name = "Nehemiah",
                NumberOfChapters = 13,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "Esth",
                Name = "Esther",
                NumberOfChapters = 10,
                Group = HistoricalBooks,
            },
            new Book
            {
                DbpId = "Job",
                Name = "Job",
                NumberOfChapters = 42,
                Group = PoetryAndWisdom,
            },
            new Book
            {
                DbpId = "Ps",
                Name = "Psalms",
                NumberOfChapters = 150,
                Group = PoetryAndWisdom,
            },
            new Book
            {
                DbpId = "Prov",
                Name = "Proverbs",
                NumberOfChapters = 31,
                Group = PoetryAndWisdom,
            },
            new Book
            {
                DbpId = "Eccl",
                Name = "Ecclesiastes",
                NumberOfChapters = 12,
                Group = PoetryAndWisdom,
            },
            new Book
            {
                DbpId = "Song",
                Name = "Song of Solomon",
                NumberOfChapters = 8,
                Group = PoetryAndWisdom,
            },
            new Book
            {
                DbpId = "Isa",
                Name = "Isaiah",
                NumberOfChapters = 66,
                Group = MajorProphets,
            },
            new Book
            {
                DbpId = "Jer",
                Name = "Jeremiah",
                NumberOfChapters = 52,
                Group = MajorProphets,
            },
            new Book
            {
                DbpId = "Lam",
                Name = "Lamentations",
                NumberOfChapters = 5,
                Group = MajorProphets,
            },
            new Book
            {
                DbpId = "Ezek",
                Name = "Ezekiel",
                NumberOfChapters = 48,
                Group = MajorProphets,
            },
            new Book
            {
                DbpId = "Dan",
                Name = "Daniel",
                NumberOfChapters = 12,
                Group = MajorProphets,
            },
            new Book
            {
                DbpId = "Hos",
                Name = "Hosea",
                NumberOfChapters = 14,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Joel",
                Name = "Joel",
                NumberOfChapters = 3,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Amos",
                Name = "Amos",
                NumberOfChapters = 9,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Obad",
                Name = "Obadiah",
                NumberOfChapters = 1,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Jonah",
                Name = "Jonah",
                NumberOfChapters = 4,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Mic",
                Name = "Micah",
                NumberOfChapters = 7,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Nah",
                Name = "Nahum",
                NumberOfChapters = 3,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Hab",
                Name = "Habakkuk",
                NumberOfChapters = 3,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Zeph",
                Name = "Zephaniah",
                NumberOfChapters = 3,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Hag",
                Name = "Haggai",
                NumberOfChapters = 2,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Zech",
                Name = "Zechariah",
                NumberOfChapters = 14,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Mal",
                Name = "Malachi",
                NumberOfChapters = 4,
                Group = MinorProphets,
            },
            new Book
            {
                DbpId = "Matt",
                Name = "Matthew",
                NumberOfChapters = 28,
                Group = GospelsAndActs,
            },
            new Book
            {
                DbpId = "Mark",
                Name = "Mark",
                NumberOfChapters = 16,
                Group = GospelsAndActs,
            },
            new Book
            {
                DbpId = "Luke",
                Name = "Luke",
                NumberOfChapters = 24,
                Group = GospelsAndActs,
            },
            new Book
            {
                DbpId = "John",
                Name = "John",
                NumberOfChapters = 21,
                Group = GospelsAndActs,
            },
            new Book
            {
                DbpId = "Acts",
                Name = "Acts",
                NumberOfChapters = 28,
                Group = GospelsAndActs,
            },
            new Book
            {
                DbpId = "Rom",
                Name = "Romans",
                NumberOfChapters = 16,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "1Cor",
                Name = "First Corinthians",
                NumberOfChapters = 16,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "2Cor",
                Name = "Second Corinthians",
                NumberOfChapters = 13,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "Gal",
                Name = "Galatians",
                NumberOfChapters = 6,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "Eph",
                Name = "Ephesians",
                NumberOfChapters = 6,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "Phil",
                Name = "Philippians",
                NumberOfChapters = 4,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "Col",
                Name = "Colossians",
                NumberOfChapters = 4,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "1Thess",
                Name = "First Thessalonians",
                NumberOfChapters = 5,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "2Thess",
                Name = "Second Thessalonians",
                NumberOfChapters = 3,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "1Tim",
                Name = "First Timothy",
                NumberOfChapters = 6,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "2Tim",
                Name = "Second Timothy",
                NumberOfChapters = 4,
                Group = PaulsLetters,
            },
            new Book
            {
                DbpId = "Titus",
                Name = "Titus",
                NumberOfChapters = 3,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "Phlm",
                Name = "Philemon",
                NumberOfChapters = 1,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "Heb",
                Name = "Hebrews",
                NumberOfChapters = 13,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "Jas",
                Name = "James",
                NumberOfChapters = 5,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "1Pet",
                Name = "First Peter",
                NumberOfChapters = 5,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "2Pet",
                Name = "Second Peter",
                NumberOfChapters = 3,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "1John",
                Name = "First John",
                NumberOfChapters = 5,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "2John",
                Name = "Second John",
                NumberOfChapters = 1,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "3John",
                Name = "Third John",
                NumberOfChapters = 1,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "Jude",
                Name = "Jude",
                NumberOfChapters = 1,
                Group = GeneralLetters,
            },
            new Book
            {
                DbpId = "Rev",
                Name = "Revelation",
                NumberOfChapters = 22,
                Group = Apocalyptic,
            },
        });
    }
}
