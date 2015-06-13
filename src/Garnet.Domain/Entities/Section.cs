using Garnet.Domain.Interfaces;
using System;

namespace Garnet.Domain.Entities
{
    public class Section : INamed
    {
        public string Name { get; set; }
        public SectionGroup Group { get; set; }
        public Uri AudioUrl { get; set; }
    }
}
