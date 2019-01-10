﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using LinkFetcher.Models;

namespace LinkFetcher
{
    public class FSharePhim
    {
        private readonly string username;
        private readonly string password;
        private CookieContainer cookieContainer;
        private HttpMessageHandler messageHandler;

        public FSharePhim(string username, string password)
        {
            this.username = username;
            this.password = password;
            this.cookieContainer = new CookieContainer();
            this.messageHandler = new HttpClientHandler() { CookieContainer = cookieContainer, AutomaticDecompression= DecompressionMethods.GZip };

        }
        public async Task<string> Get(string url)
        {
            using (var httpClient = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate

            }))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                request.Headers.TryAddWithoutValidation("referer", "https://fsharephim.com/genre/hoat-hinh/");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                request.Headers.TryAddWithoutValidation("origin", "https://fsharephim.com");
                request.Headers.TryAddWithoutValidation("cache-control", "no-cache,no-cache");
                request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                request.Headers.TryAddWithoutValidation("authority", "fsharephim.com");


                var res = await httpClient.SendAsync(request);
                var html = await res.Content.ReadAsStringAsync();

                return html;
            }
        }
        public async Task<bool> Login()
        {
            Console.WriteLine("Please waiting for login.....");
            using (var httpClient = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate

            }))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://fsharephim.com/wp-login.php");

                //request.Headers.TryAddWithoutValidation("Postman-Token", "f5e065bc-84de-402c-9d27-8e89964afa4c");
                //request.Headers.TryAddWithoutValidation("cookie", "starstruck_e2ca4943e4e06294ca16788bc8f192fe=439e44363304a894b3c9338a6c5c6e81; _ga=GA1.2.950692071.1545516189; wordpress_test_cookie=WP+Cookie+check; __cfduid=d792b38a1268fab9a32c03964bf88ee3c1546132575; _gid=GA1.2.974487134.1546259443; _gat=1");
                request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                //request.Headers.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
                request.Headers.TryAddWithoutValidation("referer", "https://fsharephim.com/genre/hoat-hinh/");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                request.Headers.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("origin", "https://fsharephim.com");
                request.Headers.TryAddWithoutValidation("cache-control", "no-cache,no-cache");
                request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                request.Headers.TryAddWithoutValidation("authority", "fsharephim.com");
                //request.AddParameter("application/x-www-form-urlencoded", "log=samuraitruong&pwd=%2540%2540ABcd123%2524%2524&redirect_to=https%253A%252F%252Ffsharephim.com%252Fgenre%252Fhoat-hinh%252F", ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                    {"log", username},
                    {"pwd", password},
                    {"redirect_to", "https://fsharephim.com/my-account/"}
                });

                var res = await httpClient.SendAsync(request);
                var html = await res.Content.ReadAsStringAsync();

                return html.Contains("Truong Nguyen");
            }
        }

        public async Task<List<Item>> GetLinks(string url, int page =0)
        {
            var url1 = page == 0 ? url : url.TrimEnd('/') + "/page/" + page.ToString() +"/";
            string html = "";
            var result = new ConcurrentBag<Item>();
            try
            {
                html = await Get(url1);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var nodes = doc.DocumentNode.SelectNodes("//*[@class=\"items\"]/article");
                foreach (HtmlNode node in nodes)
                {
                    var a = node.SelectSingleNode("div/h3/a");
                    result.Add(new Item() { Url = a.GetAttributeValue("href", ""), Name = a.InnerText.Trim() });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error" + ex.Message);
                Console.WriteLine("Error at: " + url1);
            }

            if (page == 0)
            {
                var matches = Regex.Match(html, "Page 1 of (\\d*)", RegexOptions.Multiline);
                if (matches != null && matches.Groups.Count >= 2 && !string.IsNullOrEmpty(matches.Groups[1].Value)) {
                    int pageCount = Convert.ToInt32(matches.Groups[1].Value);
                    Parallel.ForEach(Enumerable.Range(2, pageCount - 1), (item) =>
                      {
                          var subPageItems = GetLinks(url, item).Result;
                          subPageItems.ForEach(x => result.Add(x));
                      });
                }

            }
            return result.ToList();
        }
        public async Task<string> Post(string url, Dictionary<string, string> data, string referer ="https://fsharephim.com")
        {
            using (var httpClient = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate

            }))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.TryAddWithoutValidation("referer", referer);
                request.Headers.TryAddWithoutValidation("authority", "fsharephim.com");
                request.Headers.TryAddWithoutValidation("cache-control", "no-cache,no-cache");
                request.Headers.TryAddWithoutValidation("accept", "*/*");
                request.Headers.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded; charset=UTF-8");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                request.Headers.TryAddWithoutValidation("x-requested-with", "XMLHttpRequest");
                request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                // request.Headers.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
                request.Headers.TryAddWithoutValidation("origin", "https://fsharephim.com");
                //request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", "get_list_links_nonce=3d5e76e9d1&dt_string=movMC2ia19323&type=Download&action=get_list_link", ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                request.Content = new FormUrlEncodedContent(data);
                var res = await httpClient.SendAsync(request);
                var html = await res.Content.ReadAsStringAsync();
                return html;
            }

        }
        public async Task<MovieItem> FetchMovie(string url, bool recursive = true)
        {
            Console.WriteLine("----- Movie: " + url);
            string html = "";
            var result = new ConcurrentBag<string>();
            try
            {
                html = await Get(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var title = doc.DocumentNode.SelectSingleNode("//h1").InnerText.Trim();
                var genreNodes = doc.DocumentNode.SelectNodes("//*[@class='sgeneros']//a");
                var content = doc.DocumentNode.SelectSingleNode("//*[@class='wp-content']/p");
                var imagesNodes = doc.DocumentNode.SelectNodes("//*[@id=\"dt_galery\"]//a");
                var poster = doc.DocumentNode.SelectSingleNode("//*[@class='poster']/img");
                var trailer = doc.DocumentNode.SelectSingleNode("//*[@class='rptss']");
                var item = new MovieItem()
                {
                    Url = url,
                    Name = title,
                    Intro = content?.InnerText?.Trim(),
                    Poster = poster?.GetAttributeValue("src", ""),
                    Trailer = trailer?.GetAttributeValue("src", "")
                };
                if(!string.IsNullOrEmpty(item.Trailer))
                {
                    item.Trailer = HttpUtility.UrlDecode(item.Trailer);
                }
                if (genreNodes != null)
                {
                    item.Genres = genreNodes.Cast<HtmlNode>().Select(x => new Item()
                    {
                        Url = x.GetAttributeValue("href", ""), 
                        Name = x.InnerText.Trim() 
                    }).ToList();
                }
                if(imagesNodes != null)
                {
                    item.Images = imagesNodes.Cast<HtmlNode>().Select(x => new ImageItem()
                    {
                        Url = x.GetAttributeValue("href", "").Trim('\r'),
                        Name = x.GetAttributeValue("title", "").Trim(),
                        Thumbnail = x.SelectSingleNode("img")?.GetAttributeValue("src","").Trim()
                    }).ToList();
                }
                var ss = doc.DocumentNode.SelectNodes("//*[@class='se-c']");
                if (ss != null && recursive)
                {
                    var seasons = new List<Season>();
                    foreach (HtmlNode s in ss)
                    {
                        var season = new Season() {
                            Name = s.SelectSingleNode("div/span[@class='title']").InnerText.Trim()
                         };

                        var episodes = s.SelectNodes("div//*[@class='episodiotitle']/a");
                        if(episodes != null &&  recursive)
                        {
                            item.Links = episodes.Cast<HtmlNode>().Select(x => new Link()
                            {
                                Name = x.InnerText,
                                Url = x.GetAttributeValue("href", "")
                            }).ToList();
                            ConcurrentBag<MovieItem> episodeItems = new ConcurrentBag<MovieItem>();
                            Parallel.ForEach(item.Links, ep =>
                            {
                                var epMovie = FetchMovie(ep.Url, false).Result;
                                episodeItems.Add(epMovie);
                            });
                            season.Episodes = episodeItems.OrderBy(x => x.Name).ToList();

                        }

                        seasons.Add(season);
                    }

                    item.Seasons = seasons;
                }


                var match = Regex.Match(html, "get_list_links_nonce: '(.*)',dt_string: '(.*)',type: 'Download',action: 'get_list_link'");
                if (match.Success)
                {
                    html = await Post("https://fsharephim.com/wp-admin/admin-ajax.php",

                        new Dictionary<string, string>()
                    {
                    { "get_list_links_nonce", match.Groups[1].Value },
                    { "dt_string", match.Groups[2].Value },
                    { "type", "Download"},
                    { "action", "get_list_link" }

                    });
                    item.Id = match.Groups[2].Value;
                    doc.LoadHtml(html);
                    var trs = doc.DocumentNode.SelectNodes("//tbody/tr");
                    if (trs != null)
                    {
                        foreach (HtmlNode node in trs)
                        {
                            var a = node.SelectSingleNode("td/a");
                            var quantity = node.SelectSingleNode("td/strong[@class='quality']");
                            var size = node.SelectSingleNode("td[4]");

                            item.Links.Add(new Link()
                            {
                                OriginalLink = a.GetAttributeValue("href", ""),
                                Name = quantity.InnerText.Trim(),
                                Size = size.InnerText.Trim()
                            });
                            Parallel.ForEach(item.Links, link =>
                            {
                                link.Url = ResolveLink(link.OriginalLink).Result;

                                var info = FShare.GetFShareFileInfo(link.Url).Result;
                                if (info != null)
                                {
                                    link.FileSize = info.Size;
                                    link.IsAlive = true;
                                    link.Name = info.Name;
                                }
                            });
                        }
                    }
                }


                return item;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex.Message + "["  + ex.StackTrace +"] ====> " + url);
            }
            return null;
        }
        private async Task<string> ResolveLink(string url)
        {
            var html = await Get(url);
            var match = Regex.Match(html, "<a id=\"button-url\" href=\"([^\"]+)\">");
            return match.Groups[1].Value;
        }
        public async Task<List<Item>> GetGenreLinks()
        {
            var html = await Get("https://fsharephim.com");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes("//*[@id='main_header']/li[1]/ul//a");
            return nodes.Cast<HtmlNode>().Select(x => new Item() { Url = x.GetAttributeValue("href", ""), Name = x.InnerText.Trim() }).ToList();
        }
        public async Task<List<MovieItem>> FetchCategory(string url, int threads = 10)
        {
            Console.WriteLine("*** Reading category movie list....");
            var links = await GetLinks(url);
            Console.WriteLine($"Found  {links.Count} movies in this category");
            ConcurrentBag<MovieItem> items = new ConcurrentBag<MovieItem>();
            Parallel.ForEach(links,new ParallelOptions() { MaxDegreeOfParallelism = threads }, link =>
            {
                int retry = 3;
                do
                {
                    Console.WriteLine($"### Reading info from {link} | attempt # {(4 - retry)}");
                    var filename = "data/" + link.Name + ".json";
                    if (File.Exists(filename)) return;
                    var item = FetchMovie(link.Url).Result;
                    if (item == null)
                    {
                        retry--;
                        continue;
                    }
                    retry = 0;
                    items.Add(item);
                    item.Dump();
                    item.WriteTo(filename);
                } while (retry > 0);
            });
            return items.ToList();
        }


    }
}