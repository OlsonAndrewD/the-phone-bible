using Garnet.Domain.Entities;

namespace Garnet.Domain.Extensions
{
    public static class BookGroupExtensions
    {
        public static BookGroup GetTopmostAncestor(this BookGroup group)
        {
            return group.Parent == null ? group : group.Parent.GetTopmostAncestor();
        }
    }
}
