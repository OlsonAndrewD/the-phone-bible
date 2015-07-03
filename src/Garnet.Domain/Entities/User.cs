namespace Garnet.Domain.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public int ChapterNumber { get; set; }
        public string AudioVolumeCode { get; set; }
        public bool IsDramaticAudioSelected { get; set; }
    }
}
