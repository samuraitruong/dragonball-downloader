using System;
namespace ConceptDownloader.Models
{
    public class DownloadableItem
    {
        public DownloadableItem() { }
        public DownloadableItem(string url)
        {
            this.Url = url;
        }
        public string Url { get; set; }
        public double Size { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}
