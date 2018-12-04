using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConceptDownloader
{
    public class CrawlItem
    {
        public string Url { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
    }

    public class SimpleCrawler
    {
        public SimpleCrawler()
        {
        }
        public static async Task<List<CrawlItem>> GetLinks(string url)
        {
            List<CrawlItem> results = null;
            using (var client = new WebClient())
            {
                var html = await client.DownloadStringTaskAsync(new Uri(url));
                var matches = Regex.Matches(html, "href=\"([^\"]*)");
                results= matches.Cast<Match>().Select(x => new CrawlItem()
                {
                    Url = url + x.Groups[1].Value,
                    Name = x.Groups[1].Value.Trim()
                }).ToList();
            }
            return results.Where(x => x.Name!= "../").ToList();
        }
    }
}
