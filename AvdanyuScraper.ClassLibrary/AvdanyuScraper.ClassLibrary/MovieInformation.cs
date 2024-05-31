using System;
using System.Collections.Generic;

namespace AvdanyuScraper.ClassLibrary
{
    public class MovieInformation
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public List<string> Actress { get; set; }
        public List<string> Danyus { get; set; }
        public string Director { get; set; }
        public string Set { get; set; }
        public string Studio { get; set; }
        public List<string> Tag { get; set; }
        public List<string> Genres { get; set; }
        public string ReleaseDate { get; set; }

    }
}
