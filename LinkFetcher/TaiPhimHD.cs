using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LinkFetcher.Models;

namespace LinkFetcher
{
    public class TaiPhimHD
    {
        private readonly string username;
        private readonly string password;
        private CookieContainer cookieContainer;
        public TaiPhimHD(string username, string password)
        {
            this.username = username;
            this.password = password;
            this.cookieContainer = new CookieContainer();
        }

        public async Task<string> Get(string url, int retry =0)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler()
                {
                    CookieContainer = this.cookieContainer,
                })
                { 
                Timeout =TimeSpan.FromMinutes(1)
                }) 
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.TryAddWithoutValidation("cookie", "_ga=GA1.2.1112693365.1546501871; _gid=GA1.2.1831978678.1546501871; G_ENABLED_IDPS=google;");
                    request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                    //request.Headers.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
                    request.Headers.TryAddWithoutValidation("referer", "https://taiphimhd.com/");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                    // request.Headers.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("origin", "https://taiphimhd.com");
                    request.Headers.TryAddWithoutValidation("cache-control", "no-cache,no-cache");
                    request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                    request.Headers.TryAddWithoutValidation("authority", "taiphimhd.com");

                    var res = await client.SendAsync(request);
                    return await res.Content.ReadAsStringAsync();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR Get content: " + url);
                if (retry < 3) return await Get(url, retry + 1);
            }
            return null;
        }
        public async Task<string> Post(string url, Dictionary<string, string> data, int retry =0)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler()
                {
                    CookieContainer = this.cookieContainer
                }))
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.TryAddWithoutValidation("cookie", "_ga=GA1.2.1112693365.1546501871; _gid=GA1.2.1831978678.1546501871; G_ENABLED_IDPS=google;");
                    request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                    //request.Headers.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
                    request.Headers.TryAddWithoutValidation("referer", "https://taiphimhd.com/");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                    // request.Headers.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("origin", "https://taiphimhd.com");
                    request.Headers.TryAddWithoutValidation("cache-control", "no-cache,no-cache");
                    request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                    request.Headers.TryAddWithoutValidation("authority", "taiphimhd.com");
                    request.Content = new FormUrlEncodedContent(data);
                    var res = await client.SendAsync(request);
                    var html = await res.Content.ReadAsStringAsync();
                    File.WriteAllText("debug.html", html);
                    return html;
                }
            }
            catch(Exception ex)
            {
                if (retry < 3) return await Post(url, data, retry + 1);
            }
            return null;
        }
        public async Task <bool> Login()
        {
            Console.WriteLine("Loging in to taiphimhhd.com");
            await Get("https://taiphimhd.com/");
            var data = new Dictionary<string, string>()
            {
                { "login", this.username},
                { "password", this.password},
                { "remember", "1"},
                { "cookie_check", "1"},
                { "_xfToken", ""},
                { "redirect", "/"}
            };
            var html = await Post("https://taiphimhd.com/login/login", data);
            var result= html.Contains("/samuraitruong.88442");
            Console.WriteLine($"Loging status: {result}");
            return result;

        }
        public async Task<List<Item>> GetLinks(string url, int page =1)
        {
            var requestUrl = url.TrimEnd('/') + "/page-" + page;
            Console.WriteLine("--- Reading list topics from " + requestUrl);
            var html = await Get(requestUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            ConcurrentBag<Item> items = new ConcurrentBag<Item>();
            var nodes = doc.DocumentNode.SelectNodes("//*[@class='discussionListItems']//a[@class='PreviewTooltip']");
            var result =  nodes.Cast<HtmlNode>().Select(x => new Item()
            {
                Name = x.InnerText.Trim(),
                Url = "https://taiphimhd.com/"+ x.GetAttributeValue("href", "")
            }).ToList();

            result.ForEach(x => items.Add(x));
            if(page == 1)
            {
                var match = Regex.Match(html, "Trang 1 của (\\d*) trang");
                if(!string.IsNullOrEmpty(match.Value))
                {
                    var totalPage = Convert.ToInt32(match.Groups[1].Value);
                    Parallel.ForEach(Enumerable.Range(2, totalPage-1), pn =>
                    {
                        var subList = GetLinks(url, pn).Result;
                        subList.ForEach(x => items.Add(x));
                    });

                }
            }
            return items.ToList();
        }

        public async Task<MovieItem> GetMovieLinks(string url)
        {
            try
            {
                Console.WriteLine("### Reading topic  details: " + url);
                var html = await Get(url);
                File.WriteAllText("debug.html", html);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                // find the thanks button
                var match = Regex.Match(html, "posts/(\\d*)/like");
                var movies = new MovieItem()
                {
                    Name = doc.DocumentNode.SelectSingleNode("//h1").InnerText.Trim(),
                    Id = match.Groups[1].Value,
                    Url = url
                };
                var token = doc.DocumentNode.SelectSingleNode("//*[@name='_xfToken']").GetAttributeValue("value", "");
                var likeNode = doc.DocumentNode.SelectSingleNode("//*[@class='publicControls']/a[2]");

                var data = new Dictionary<string, string>()
                {
                    {"_xfNoRedirect","1"},
                    {"_xfToken",token},
                    {"_xfResponseType","json"},
                    {"_xfRequestUri",url.Replace("https://taiphimhd.com/", string.Empty)},

                };

                if (!likeNode.HasClass("unlike"))
                {
                    Console.WriteLine("Liking topic post...");

                    var json1 = await Post($"https://taiphimhd.com/posts/{movies.Id}/like", data);
                }

                Console.Write("... Getting secured content");


                var json = await Post($"https://taiphimhd.com/posts/{movies.Id}/like-hide-check", data);
                var html1 = XenForoLikeResponse.FromJson(json).MessagesTemplateHtml.First().Value;

                var pattern = "https?:\\/\\/w?w?w?\\.?fshare\\.vn\\/file\\/([A-Z0-9]*)";
                var matches = Regex.Matches(html1 + html, pattern);
                movies.Links = matches.Cast<Match>().Select(x => new Link()
                {
                    Url = x.Value.Trim()
                }).ToList();

                //fetch folder
                pattern = "https?:\\/\\/w?w?w?\\.?fshare\\.vn\\/folder\\/([A-Z0-9]*)";
                matches = Regex.Matches(html1 + html, pattern);
                foreach (Match folder in matches)
                {
                    var response = await FShare.GetFilesInFolder(folder.Value.Trim());

                    if (response != null)
                    {
                        response.Items.ForEach(x => movies.Links.Add(new Link()
                        {
                            Url = x.Url,
                            FileSize = x.Size,
                            Size = x.Size.ToHumandReadable(),
                            Name = x.Name,
                            IsAlive = true
                        }));
                    }
                }
                Parallel.ForEach(movies.Links.Where(x => x.FileSize == 0), link =>
                 {
                     var li = FShare.GetFShareFileInfo(link.Url).Result;
                     if (li != null)
                     {
                         link.FileSize = li.Size;
                         link.Name = li.Name;
                         link.IsAlive = true;
                         link.Size = li.Size.ToHumandReadable();
                         link.OriginalLink = link.Url;
                     }
                 });
                //File.WriteAllText("debug.json", html);
                return movies;
            }
            catch(Exception ex)
            {
                Console.WriteLine("### ERROR ###" + url);
                return null;
            }
        }

        public async Task<List<MovieItem>> GetTopicPosts(string topicUrl)
        {
            var list = await GetLinks(topicUrl);
            ConcurrentBag<MovieItem> movieItems = new ConcurrentBag<MovieItem>();
            Parallel.ForEach(list, topic =>
            {
                var movie = GetMovieLinks(topic.Url).Result;
                if (movie != null)
                {
                    movieItems.Add(movie);
                    movie.Dump();
                    movie.WriteTo("taiphimhd/hoathinh/" + movie.Name + ".json");
                }
            });

            return movieItems.ToList();
        }

    }
}
