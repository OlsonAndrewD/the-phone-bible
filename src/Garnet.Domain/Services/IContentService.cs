using Garnet.Domain.Entities;
using System.Collections.Generic;

namespace Garnet.Domain.Services
{
    public interface IContentService
    {
        string SectionDescriptor { get; }
        string GetDefaultSectionId();
        string GetContentUrl(string sectionId);
        string GetSectionAfter(string sectionId);
        SectionGroup GetGroup(string groupName);
        IEnumerable<SectionGroup> GetChildGroups(string parentGroupName);
        IEnumerable<Section> GetSectionsInGroup(string groupName);
    }
}
