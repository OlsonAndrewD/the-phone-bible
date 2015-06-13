namespace Garnet.Domain.Entities
{
    public class BookGroup
    {
        public string Name { get; set; }
        public BookGroup Parent { get; set; }
    }
}
