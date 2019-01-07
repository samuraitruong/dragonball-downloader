using System;
using System.Collections.Generic;

namespace LinkFetcher.Models
{
    public class Item
    {
        public string Url { get; set; }
        public string Name { get; set; }
    }
    public class ImageItem : Item
    {
        public string Thumbnail { get; set; }
    }
    public class Link : Item
    {
        public string OriginalLink { get; set; }

        public string Size { get; set; }
        public long FileSize { get; set; }
        public bool IsAlive { get; set; }

    }
    public class Season : Item {
        public List<MovieItem> Episodes { get; set; }

    }
    public class MovieItem : Item
    {
        public string Id { get; set; }
        public List<Item> Genres { get; set; }
        public List<ImageItem> Images { get; set; }
        public MovieItem()
        {
            this.Images = new List<ImageItem>();
            this.Genres = new List<Item>();
            this.Links = new List<Link>();
        }

        public List<Link> Links { get; set; }
        public string Intro { get; set; }
        public string Poster { get; set; }
        public string Trailer { get;  set; }
        public List<Season> Seasons { get; set; }
    }
}
