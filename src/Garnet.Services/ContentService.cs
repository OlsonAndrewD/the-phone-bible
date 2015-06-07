using System;
using Garnet.Domain.Services;

namespace Garnet.Services
{
    public class ContentService : IContentService
    {
        public string GetDefaultSectionId()
        {
            return "John|1";
        }

        public string GetSectionAfter(string sectionId)
        {
            return sectionId == "John|1" ? "John|2" : "John|1";
        }

        public string GetContentUrl(string sectionId)
        {
            switch (sectionId)
            {
                case "John|1":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___01_John________ENGESVN2DA.mp3";
                case "John|2":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___02_John________ENGESVN2DA.mp3";
            }

            throw new NotSupportedException("Content not found");
        }
    }
}
