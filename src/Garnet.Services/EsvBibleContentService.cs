using System;
using System.Collections.Generic;
using System.Linq;
using Garnet.Domain.Entities;
using Garnet.Domain.Services;

namespace Garnet.Services
{
    public class EsvBibleContentService : IContentService
    {
        public string SectionDescriptor { get { return "chapter"; } }

        public string GetDefaultSectionId()
        {
            return "John|1";
        }

        public string GetSectionAfter(string sectionId)
        {
            var segments = sectionId.Split('|');
            if(segments[0] == "John")
            {
                var chapterNumber = int.Parse(segments[1]);
                if (chapterNumber == 21)
                {
                    chapterNumber = 1;
                }
                else
                {
                    chapterNumber = chapterNumber + 1;
                }

                return "John|" + chapterNumber.ToString();
            }

            return "John|1";
        }

        public string GetContentUrl(string sectionId)
        {
            switch (sectionId)
            {
                case "John|1":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___01_John________ENGESVN2DA.mp3";
                case "John|2":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___02_John________ENGESVN2DA.mp3";
                case "John|3":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___03_John________ENGESVN2DA.mp3";
                case "John|4":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___04_John________ENGESVN2DA.mp3";
                case "John|5":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___05_John________ENGESVN2DA.mp3";
                case "John|6":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___06_John________ENGESVN2DA.mp3";
                case "John|7":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___07_John________ENGESVN2DA.mp3";
                case "John|8":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___08_John________ENGESVN2DA.mp3";
                case "John|9":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___09_John________ENGESVN2DA.mp3";
                case "John|10":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___10_John________ENGESVN2DA.mp3";
                case "John|11":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___11_John________ENGESVN2DA.mp3";
                case "John|12":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___12_John________ENGESVN2DA.mp3";
                case "John|13":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___13_John________ENGESVN2DA.mp3";
                case "John|14":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___14_John________ENGESVN2DA.mp3";
                case "John|15":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___15_John________ENGESVN2DA.mp3";
                case "John|16":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___16_John________ENGESVN2DA.mp3";
                case "John|17":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___17_John________ENGESVN2DA.mp3";
                case "John|18":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___18_John________ENGESVN2DA.mp3";
                case "John|19":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___19_John________ENGESVN2DA.mp3";
                case "John|20":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___20_John________ENGESVN2DA.mp3";
                case "John|21":
                    return "http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___21_John________ENGESVN2DA.mp3";
            }

            return null;
        }

        public SectionGroup GetGroup(string groupName)
        {
            return SectionGroups.FirstOrDefault(x => x.Name == groupName);
        }

        public IEnumerable<SectionGroup> GetChildGroups(string parentGroupName)
        {
            if (parentGroupName == null)
            {
                return SectionGroups.Where(x => x.Parent == null);
            }
            else
            {
                return SectionGroups.Where(x => x.Parent != null && x.Parent.Name == parentGroupName);
            }
        }

        public IEnumerable<Section> GetSectionsInGroup(string groupName)
        {
            return Sections.Where(x => x.Group.Name == groupName);    
        }

        private static readonly SectionGroup OldTestament = new SectionGroup { Name = "Old Testament" };
        private static readonly SectionGroup BooksOfMoses =
            new SectionGroup { Parent = OldTestament, Name = "Books of Moses" };
        private static readonly SectionGroup HistoricalBooks = 
            new SectionGroup { Parent = OldTestament, Name = "Historical Books" };
        private static readonly SectionGroup PoetryAndWisdom = 
            new SectionGroup { Parent = OldTestament, Name = "Poetry and Wisdom" };
        private static readonly SectionGroup MajorProphets = 
            new SectionGroup { Parent = OldTestament, Name = "Major Prophets" };
        private static readonly SectionGroup MinorProphets = 
            new SectionGroup { Parent = OldTestament, Name = "Minor Prophets" };

        private static readonly SectionGroup NewTestament = new SectionGroup { Name = "New Testament" };
        private static readonly SectionGroup GospelsAndActs = 
            new SectionGroup { Parent = NewTestament, Name = "Gospels and Acts" };
        private static readonly SectionGroup PaulsLetters = 
            new SectionGroup { Parent = NewTestament, Name = "Paul's Letters" };
        private static readonly SectionGroup GeneralLettersAndProphecy = 
            new SectionGroup { Parent = NewTestament, Name = "General Letters and Prophesy" };

        private static readonly IEnumerable<SectionGroup> SectionGroups = new[]
        {
            OldTestament,
            BooksOfMoses,
            HistoricalBooks,
            PoetryAndWisdom,
            MajorProphets,
            MinorProphets,

            NewTestament,
            GospelsAndActs,
            PaulsLetters,
            GeneralLettersAndProphecy
        };

        private static readonly IEnumerable<Section> Sections = new[]
        {
            new Section
            {
                Name = "Genesis",
                Group = BooksOfMoses,
            },
            new Section
            {
                Name = "Exodus",
                Group = BooksOfMoses,
            },
            new Section
            {
                Name = "First Chronicles",
                Group = HistoricalBooks,
            },
            new Section
            {
                Name = "Second Chronicles",
                Group = HistoricalBooks,
            },
            new Section
            {
                Name = "Psalms",
                Group = PoetryAndWisdom,
            },
            new Section
            {
                Name = "Proverbs",
                Group = PoetryAndWisdom,
            },
            new Section
            {
                Name = "Matthew",
                Group = GospelsAndActs,
            },
            new Section
            {
                Name = "Mark",
                Group = GospelsAndActs,
            },
        };
    }
}
