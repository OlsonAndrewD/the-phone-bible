﻿namespace Garnet.Domain.Entities
{
    public class Book
    {
        public string Name { get; set; }
        public int NumberOfChapters { get; set; }
        public BookGroup Group { get; set; }
    }
}
