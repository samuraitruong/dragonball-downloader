using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Console = Colorful.Console;
using ConceptDownloader.Models;
using System.Net.Http;

namespace ConceptDownloader
{


    public class SimpleCrawler
    {
        private readonly ApplicationArguments options;

        public SimpleCrawler(ApplicationArguments options = null)
        {
            this.options = options;
        }
        public  string ExtractName(string name)
        {
            if (this.options != null && this.options.DownloadAll) return name;
            var replacedInput = HttpUtility.UrlDecode(name.ToLower());
            replacedInput = replacedInput.Replace(".bluray", string.Empty);
            replacedInput = replacedInput.Replace(".fullhd", string.Empty);
            replacedInput = replacedInput.Replace(".full.hd", string.Empty);
            replacedInput = replacedInput.Replace(".repack", string.Empty);
            replacedInput = replacedInput.Replace(".unrated", string.Empty);
            replacedInput = replacedInput.Replace(".extended", string.Empty);
            replacedInput = replacedInput.Replace(".internal.", ".");
            replacedInput = replacedInput.Replace(".imax", string.Empty);
            replacedInput = replacedInput.Replace(".limited", string.Empty);
            replacedInput = replacedInput.Replace("1080p", "|");
            replacedInput = replacedInput.Replace("720p", "|");
            replacedInput = replacedInput.Replace("2160p", "|"); 
            // Regex regex = new Regex("(.*)\\.(1080p|720p).*");
            // var match = regex.Match(replacedInput);
            // if (match != null)
            // {
            //     return match.Groups[1].Value.ToLower();
            // }
            return replacedInput.Split('|')[0];
            //var match = Regex.Match(name, "(.*)\\.(\\d{3,4}p).*");
            //if (match != null) return match.Groups[1].Value;
            //return name;
        }
        public  List<string> ParseLinks(string html)
        {
            string pattern = "href=\"(.*)\"";
            var matches = Regex.Matches(html, pattern);
            return matches.Cast<Match>()
            .Select(x => x.Groups[1].Value.Trim())
                .ToList();

        }
        public  async Task<List<DownloadableItem>> GetLinks(string url, bool recursive = false)
        {
            url = url.EndsWith("/", StringComparison.Ordinal) ? url : url + "/";

            Console.WriteLine("Getting content from: " + url, ConsoleColor.DarkGray);
            ConcurrentBag<DownloadableItem> results = new ConcurrentBag<DownloadableItem>();
            List<DownloadableItem> pageItems;
            using (var client = new WebClient())
            {
                var html = await client.DownloadStringTaskAsync(new Uri(url));
                string pattern = "\"(.*)\".*\\s\\s\\s(\\d{6}\\d*)";
                var matches = Regex.Matches(html, pattern);
                pageItems = matches.Cast<Match>().Select(x => new DownloadableItem()
                {
                    Url = url + x.Groups[1].Value,
                    Name = x.Groups[1].Value.Trim(),
                    Size = Convert.ToDouble(x.Groups[2].Value),
                    ShortName = ExtractName(x.Groups[1].Value.Trim())
                }).ToList();
                if(recursive)
                {
                    var folders = ParseLinks(html);
                    folders = folders.Where(f => !f.Contains("../") && f.EndsWith("/", StringComparison.Ordinal)).ToList();

                    Parallel.ForEach(folders, new ParallelOptions() { MaxDegreeOfParallelism =10 }, (folder) =>
                     {
                         var childUrl = url + folder;
                         var childPageItems = GetLinks(childUrl, recursive).Result;
                         childPageItems.ForEach(x => results.Add(x));
                     });
                }
            }
            pageItems = pageItems.Where(x => x.Name != "../").ToList();
            pageItems = pageItems.GroupBy(x => x.ShortName, (key, group) => group.OrderByDescending(y => y.Size).FirstOrDefault()).ToList();
            pageItems.Reverse();
            pageItems.ForEach(x => results.Add(x));
            return results.ToList();
        }

        public async Task<List<DownloadableItem>> ExtractLinks(string url, params string[] filters)
        {
            var result = new List<DownloadableItem>();

            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);

                var matches = Regex.Matches(html, "https?:\\/\\/(www.)?([a-zA-Z0-9-_]*.[a-z-0-9]{2,5})(\\/.*)?");
                if(matches != null)
                {
                    result = matches.Cast<Match>()
                        .Select(x => new DownloadableItem() { Url = x.Value })
                        .ToList();
                }
            }
            //filter
            if(filters.Length >0)
            {
                result = result.Where(x => filters.Any(x.Url.Contains))
                .ToList();
            }
            return result;
        }
    }
}
