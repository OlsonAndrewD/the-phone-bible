using System;
using Garnet.Services.Interfaces;

namespace Garnet.Services.Implementations
{
    public class ContentService : IContentService
    {
        public string GetCurrentContentUrl()
        {
            return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B01___01_Matthew_____ENGESVN2DA.mp3";
        }
    }
}
