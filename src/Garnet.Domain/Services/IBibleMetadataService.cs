using Garnet.Domain.Entities;
using System.Collections.Generic;

namespace Garnet.Domain.Services
{
    public interface IBibleMetadataService
    {
        BookGroup GetGroup(string groupName);
        IEnumerable<BookGroup> GetChildGroups(string parentGroupName);

        Book GetBook(string bookName);
        IEnumerable<Book> GetBooks(string groupName = null);

        Chapter GetDefaultChapter();
        Chapter GetChapterAfter(Chapter chapter);
    }
}
