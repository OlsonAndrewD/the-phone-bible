namespace Garnet.Domain.Services
{
    public interface IContentService
    {
        string GetDefaultSectionId();
        string GetSectionAfter(string sectionId);
        string GetContentUrl(string sectionId);
    }
}
