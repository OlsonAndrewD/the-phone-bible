using Garnet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Garnet.Domain.Services
{
    public interface IContentService
    {
        Chapter GetDefaultChapter();
        Task<string> GetContentUrlAsync(Chapter chapter);
        Chapter GetChapterAfter(Chapter chapter);
        Book GetBook(string bookName);
        BookGroup GetGroup(string groupName);
        IEnumerable<BookGroup> GetChildGroups(string parentGroupName);
        IEnumerable<Book> GetBooks(string groupName = null);
    }
}
