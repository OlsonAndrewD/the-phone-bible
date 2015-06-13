using Garnet.Domain.Interfaces;
using System.Collections.Generic;

namespace Garnet.Domain.Entities
{
    public class SectionGroup : INamed
    {
        public string Name { get; set; }
        public SectionGroup Parent { get; set; }
    }
}
