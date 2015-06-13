using System.Collections.Generic;

namespace Garnet.Domain.Entities
{
    public class Volume
    {
        public string Name { get; set; }
        public IEnumerable<Section> Sections { get; set; }
        public IEnumerable<SectionGroup> SectionGroups { get; set; }
    }
}
