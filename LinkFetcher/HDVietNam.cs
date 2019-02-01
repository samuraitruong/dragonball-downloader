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
    public class HDVietNam
    {
        private readonly string username;
        private readonly string password;
        private CookieContainer cookieContainer;
        public HDVietNam(string username, string password)
        {
            this.username = username;
            this.password = password;
            this.cookieContainer = new CookieContainer();
        }

        public async Task<string> Get(string url, int retry = 0)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    CookieContainer = this.cookieContainer,
                })
                {
                    Timeout = TimeSpan.FromMinutes(3)
                })
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);

                    // request.Headers.TryAddWithoutValidation("cookie", "_ga=GA1.2.1112693365.1546501871; _gid=GA1.2.1831978678.1546501871; G_ENABLED_IDPS=google;");
                    request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                    request.Headers.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
                    request.Headers.TryAddWithoutValidation("referer", "http://www.hdvietnam.com/");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                    // request.Headers.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded");
                    //request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("origin", "www.hdvietnam.com");
                    request.Headers.TryAddWithoutValidation("cache-control", "no-cache,no-cache");
                    request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                    request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    //request.Headers.TryAddWithoutValidation("authority", "taiphimhd.com");

                    var res = await client.SendAsync(request);
                    return await res.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get content: " + url + "\tRetrying #" + retry);
                Console.WriteLine(ex.Message);
                await Task.Delay(1000 * (retry +1));
                if (retry < 5) return await Get(url, retry + 1);
            }
            return null;
        }

        public List<MovieItem> GetMovieLinks(List<Item> list)
        {
            ConcurrentBag<MovieItem> movieItems = new ConcurrentBag<MovieItem>();
            Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, topic =>
           {
               try
               {
                   var movie = GetMovieLinks(topic.Url).Result;
                   if (movie != null)
                   {
                       movieItems.Add(movie);
                       movie.Dump();
                   }
               }
               catch (Exception ex)
               {
                   Console.WriteLine("********************  ERROR  ************");
                   Console.Write(topic.Url);
                   throw ex;
               }
           });

            return movieItems.ToList();
        }

        public async Task<string> Post(string url, Dictionary<string, string> data, int retry = 0)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler()
                {
                    CookieContainer = this.cookieContainer,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                }))
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.TryAddWithoutValidation("cookie", "_ga=GA1.2.1112693365.1546501871; _gid=GA1.2.1831978678.1546501871; G_ENABLED_IDPS=google;");
                    request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                    request.Headers.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
                    request.Headers.TryAddWithoutValidation("referer", "http://www.hdvietnam.com/");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                    // request.Headers.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("origin", "http://www.hdvietnam.com/");
                    request.Headers.TryAddWithoutValidation("cache-control", "no-cache,no-cache");
                    request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                    //request.Headers.TryAddWithoutValidation("authority", "hdvietnam.com");
                    request.Content = new FormUrlEncodedContent(data);
                    var res = await client.SendAsync(request);
                    var html = await res.Content.ReadAsStringAsync();
                    return html;
                }
            }
            catch (Exception ex)
            {
                if (retry < 3) return await Post(url, data, retry + 1);
            }
            return null;
        }
        public async Task<bool> Login()
        {
            Console.WriteLine("Loging in to hdvietnam.com");
            await Get("http://www.hdvietnam.com/");
            var data = new Dictionary<string, string>()
            {
                { "login", this.username},
                { "password", this.password},
                { "remember", "1"},
                { "cookie_check", "1"},
                { "_xfToken", ""},
                { "redirect", "/"}
            };
            var html = await Post("http://www.hdvietnam.com/login/login", data);
            var pattern = "members/([^\\.]*)\\.(\\d*)/";
            var matched = Regex.Match(html, pattern);
            var result = (matched.Success && matched.Groups[1].Value == username);
            Console.WriteLine($"Loging status: {result}");
            if (!result)
            {
                pattern = "<span class=\"errors\">([^<]*)";
                matched = Regex.Match(html, pattern);

                Console.WriteLine(matched.Groups[1].Value);
            }
            return result;

        }
        public async Task<List<Item>> GetLinks(string url, int page = 1)
        {
            try
            {
                var requestUrl = url.TrimEnd('/') + "/page-" + page;
                Console.WriteLine("--- Reading list topics from " + requestUrl);
                var html = await Get(requestUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                ConcurrentBag<Item> items = new ConcurrentBag<Item>();
                var nodes = doc.DocumentNode.SelectNodes("//*[@class='discussionListItems']//a[@class='PreviewTooltip']");
                var result = nodes.Cast<HtmlNode>().Select(x => new Item()
                {
                    Name = x.InnerText.Trim(),
                    Url = "http://www.hdvietnam.com/" + x.GetAttributeValue("href", "")
                }).ToList();

                result.ForEach(x => items.Add(x));
                if (page == 1)
                {
                    var match = Regex.Match(html, "Trang 1 của (\\d*) trang");
                    if (!string.IsNullOrEmpty(match.Value))
                    {
                        var totalPage = Convert.ToInt32(match.Groups[1].Value);
                        Parallel.ForEach(Enumerable.Range(2, totalPage - 1), new ParallelOptions() { MaxDegreeOfParallelism = 8 } , pn =>
                         {
                             var subList = GetLinks(url, pn).Result;
                             subList.ForEach(x => items.Add(x));
                         });

                    }
                }
                return items.ToList();
            }
            catch(Exception ex) { }
            return new List<Item>();
        }
        public async Task<List<Item>> GetLinksIntContent(string url)
        {
            var requestUrl = url;
            Console.WriteLine("--- Reading list content from " + requestUrl);
            var html = await Get(requestUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            ConcurrentBag<Item> items = new ConcurrentBag<Item>();
            var nodes = doc.DocumentNode.SelectNodes("//*[@class='messageContent']//a");
            var result = nodes.Cast<HtmlNode>().Select(x => new Item()
            {
                Name = x.InnerText.Trim(),
                Url = x.GetAttributeValue("href", "")
            }).ToList();


            return result.Where(x => x.Url.Contains("/threads/"))
            .GroupBy(x => x.Url, (key, groupedItems) => groupedItems.FirstOrDefault())
            .ToList();
        }
        public async Task<List<string>> GetFshareLink(string sourceUrl, int page = 1)
        {
            var url = sourceUrl.TrimEnd('/') + "/page-" + page;
            try
            {
                Console.WriteLine("*** Reading topic  details: " + url);
                var html = await Get(url);
                if(string.IsNullOrEmpty(html)) throw new Exception("HTTP_ERROR");
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
                var likeNode = doc.DocumentNode.SelectNodes("//*[@id='messageList']/li//a[contains(@class, 'LikeLink')]");
                ConcurrentQueue<string> likeCount = new ConcurrentQueue<string>();
                var links = new List<string>();
                var pageNumberMatch = Regex.Match(html, "Trang 1 của (\\d*) trang");
                int totalPage = 1;
                if(pageNumberMatch.Success)
                {
                    totalPage = Convert.ToInt32(pageNumberMatch.Groups[1].Value);
                };
                var allLikeNode = likeNode.Cast<HtmlNode>();

            
                if (likeNode != null)
                {
                    Parallel.ForEach(totalPage >5? allLikeNode: allLikeNode.Take(2), new ParallelOptions() { MaxDegreeOfParallelism = 10}, item =>
                    {
                        {
                            if (item.HasClass("unlike")) return;

                            var dataContainer = item.GetAttributeValue("data-container", "");
                            var postId = dataContainer.Replace("#likes-post-", "");
                            likeCount.Enqueue(postId);
                            var data = new Dictionary<string, string>()
                                {
                                    {"_xfNoRedirect","1"},
                                    {"_xfToken",token},
                                    {"_xfResponseType","json"},
                                    {"_xfRequestUri",url.Replace("http://www.hdvietnam.com/", string.Empty)},

                                };

                            // Console.WriteLine("Like post " + postId);

                            var json1 = Post($"http://www.hdvietnam.com/posts/{postId}/like", data).Result;

                        }
                    });
                    if (likeCount.Count > 0)
                    {
                        // Console.WriteLine("Getting secured content after like all post : " + url);
                        //var json = await Post($"http://www.hdvietnam.com/posts/{movies.Id}/like-hide-check", data);
                        html = await Get(url);
if(string.IsNullOrEmpty(html)) throw new Exception("HTTP_ERROR");
                    }

                    var pattern = "https?:\\/\\/w?w?w?\\.?fshare\\.vn\\/(file|folder)\\/([A-Z0-9]*)";
                    var matches = Regex.Matches(html, pattern);
                    links = matches.Cast<Match>().Select(x => x.Value).ToList();

                }

                //fetch folder
                //pattern = "https?:\\/\\/w?w?w?\\.?fshare\\.vn\\/folder\\/([A-Z0-9]*)";
                //matches = Regex.Matches(html, pattern);


                ConcurrentBag<string> results = new ConcurrentBag<string>();
                if (page == 1)
                {
                    if (!string.IsNullOrEmpty(pageNumberMatch.Value) && pageNumberMatch.Success)
                    {
                        if (totalPage > 5) { 
                            Parallel.ForEach(Enumerable.Range(2, totalPage - 1), new ParallelOptions() { MaxDegreeOfParallelism = 5 }, pn =>
                             {
                                 var subList = GetFshareLink(sourceUrl, pn).Result;
                                 subList.ForEach(x => results.Add(x));
                             });
                        }

                    }
                }

                links.AddRange(results.ToList());
                return links.Distinct().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("#### ERROR ####" + url);
                Console.WriteLine(ex.Message + ex.StackTrace);
                if(ex.Message == "HTTP_ERROR") throw ex;
            }
            return new List<string>();

        }

        public async Task<MovieItem> GetMovieLinks(string url)
        {
            try
            {
                Console.WriteLine("### Reading topic  details: " + url);
                var html = await Get(url);
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
                    {"_xfRequestUri",url.Replace("http://www.hdvietnam.com/", string.Empty)},

                };

                if (!likeNode.HasClass("unlike"))
                {
                    Console.WriteLine("Liking topic post...");

                    var json1 = await Post($"http://www.hdvietnam.com/posts/{movies.Id}/like", data);
                }

                Console.Write("... Getting secured content");


                //var json = await Post($"http://www.hdvietnam.com/posts/{movies.Id}/like-hide-check", data);
                html = await Get(url);

                var pattern = "https?:\\/\\/w?w?w?\\.?fshare\\.vn\\/file\\/([A-Z0-9]*)";
                var matches = Regex.Matches(html, pattern);
                movies.Links = matches.Cast<Match>().Select(x => new Link()
                {
                    Url = x.Value.Trim()
                }).ToList();

                //fetch folder
                pattern = "https?:\\/\\/w?w?w?\\.?fshare\\.vn\\/folder\\/([A-Z0-9]*)";
                matches = Regex.Matches(html, pattern);
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
                return movies;
            }
            catch (Exception ex)
            {
                Console.WriteLine("### ERROR ###" + url);
                Console.WriteLine(ex.Message);
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
