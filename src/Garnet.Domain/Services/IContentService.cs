using Garnet.Domain.Entities;
using System.Collections.Generic;

namespace Garnet.Domain.Services
{
    public interface IContentService
    {
        string SectionDescriptor { get; }
        Chapter GetDefaultChapter();
        string GetContentUrl(Chapter chapter);
        Chapter GetChapterAfter(Chapter chapter);
        //SectionGroup GetGroup(string groupName);
        //IEnumerable<SectionGroup> GetChildGroups(string parentGroupName);
        //IEnumerable<Section> GetSectionsInGroup(string groupName);
    }
}
