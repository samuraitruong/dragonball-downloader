using System;
using System.Collections.Generic;

namespace LinkFetcher.Models
{

    public class Link
    {
        public string OriginalLink { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Size { get; set; }
        public long FileSize { get; set; }

    }
    public class MovieItem
    {
        public string Id { get; set; }
        public MovieItem()
        {
            this.Links = new List<Link>();
        }
        public string Url { get; set; }
        public string Name { get; set; }
        public List<Link> Links { get; set; }
    }
}
