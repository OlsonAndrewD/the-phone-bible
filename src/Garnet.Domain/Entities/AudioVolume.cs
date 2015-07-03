using Garnet.Domain.Enums;

namespace Garnet.Domain.Entities
{
    public class AudioVolume
    {
        public string Code { get; set; }
        public string VersionName { get; set; }
        public bool IsDramatic { get; set; }
        public CollectionType CollectionType { get; set; }
    }
}
