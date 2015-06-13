using Garnet.Domain.Entities;
using System.Collections.Generic;

namespace Garnet.Api
{
    public interface IBrowserFactory
    {
        IBrowser CreateGroupBrowser(BookGroup bookGroup);
        IBrowser CreateBookBrowser(Book book);
    }
}
