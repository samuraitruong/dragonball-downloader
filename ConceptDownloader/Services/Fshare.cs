using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConceptDownloader.Extensions;
using ConceptDownloader.Models;
using RestSharp;

namespace ConceptDownloader.Services
{
    [Service("fshare.vn")]
    public class Fshare  : ILinkFetcherService
    {
        private readonly string username;

        public string Password { get; }
        private CookieContainer cookieContainer;
        private bool logged = false;
        private string cookieFile;
        public Fshare(string username, string password)
        {
            this.username = username;
            Password = password;
            cookieContainer = new CookieContainer();
            cookieFile = "fshare_" + this.username.Replace("@", "_").Replace(".", "_") +".cached";
            cookieFile = Path.Combine("_cached", "cookies", cookieFile);

        }

        public async Task<string> Get(string url, bool json = false)
        {
            using (var client = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            }))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                //request.Headers.TryAddWithoutValidation("Postman-Token", "8c77f602-f706-41a3-9fda-23bbed76618e");
                //request.Headers.TryAddWithoutValidation("cookie", "fshare-app=p3k2f77n4a2as26pgpmr2ccg3p; _uidcms=1546732332542546711; _gid=GA1.2.428558864.1546732335; _gat_gtag_UA_97071061_1=1; _ga=GA1.2.lv0-%2524%2524%2520; _gat=1");
                request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                //request.Headers.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
                request.Headers.TryAddWithoutValidation("referer", "https://www.fshare.vn/");
                if (json)
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/json");
                }
                else
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            
            request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                request.Headers.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded");
                //request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("origin", "https://www.fshare.vn");
                request.Headers.TryAddWithoutValidation("cache-control", "no-cache,no-cache");
                request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                request.Headers.TryAddWithoutValidation("authority", "www.fshare.vn");
                //request.AddParameter("application/x-www-form-urlencoded", "_csrf-app=pshjdNwbCtOVYXTKaCxXf3xIVe3istxN8ggoCxL_RTnwjhQ3mlxioPRVBoBZXSM0Iylio4bckSi0Z21EWMx0fQ%253D%253D&LoginForm%255Bemail%255D=samuraitruong%2540hotmail.com&LoginForm%255Bpassword%255D=%2540bhu8*UHB%2521&LoginForm%255BrememberMe%255D=0&LoginForm%255BrememberMe%255D=1", ParameterType.RequestBody);
                var res = await client.SendAsync(request);
                var html = await res.Content.ReadAsStringAsync();
                return html;
            }
        }
        public async Task<string> Post(string url, Dictionary<string, string> data)
        {
            using(var client = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            }))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                //request.Headers.TryAddWithoutValidation("Postman-Token", "8c77f602-f706-41a3-9fda-23bbed76618e");
                //request.Headers.TryAddWithoutValidation("cookie", "fshare-app=p3k2f77n4a2as26pgpmr2ccg3p; _uidcms=1546732332542546711; _gid=GA1.2.428558864.1546732335; _gat_gtag_UA_97071061_1=1; _ga=GA1.2.lv0-%2524%2524%2520; _gat=1");
                request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                //request.Headers.TryAddWithoutValidation("accept-encoding", "gzip, deflate, br");
                request.Headers.TryAddWithoutValidation("referer", "https://www.fshare.vn/");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                request.Headers.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("origin", "https://www.fshare.vn");
                request.Headers.TryAddWithoutValidation("cache-control", "no-cache,no-cache");
                request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                request.Headers.TryAddWithoutValidation("authority", "www.fshare.vn");
                request.Content = new FormUrlEncodedContent(data);
                //request.AddParameter("application/x-www-form-urlencoded", "_csrf-app=pshjdNwbCtOVYXTKaCxXf3xIVe3istxN8ggoCxL_RTnwjhQ3mlxioPRVBoBZXSM0Iylio4bckSi0Z21EWMx0fQ%253D%253D&LoginForm%255Bemail%255D=samuraitruong%2540hotmail.com&LoginForm%255Bpassword%255D=%2540bhu8*UHB%2521&LoginForm%255BrememberMe%255D=0&LoginForm%255BrememberMe%255D=1", ParameterType.RequestBody);
                var res = await client.SendAsync(request);
                var html = await res.Content.ReadAsStringAsync();
                return html;
            }
        }
        private Tuple<string , string> ExtractToken(string html)
        {
            var paramsMatch = Regex.Match(html, "<meta name=\\\"csrf-param\\\" content=\\\"([^\"]*)\"");
            var tokenMatch = Regex.Match(html, "<meta name=\\\"csrf-token\\\" content=\\\"([^\"]*)\"");
            var token = tokenMatch.Groups[1].Value.Trim();
            var param = paramsMatch.Groups[1].Value.Trim();
            return new Tuple<string, string>(param, token);
        }
        public async Task<bool > Login(bool noCache= false)
        {
            var fi = new FileInfo(cookieFile);
            if (fi.Exists && !noCache)
            {
                cookieContainer = cookieContainer.Load(cookieFile);
                return true;
            }
            else
            {
                var html = await Get("https://www.fshare.vn");
                var token = ExtractToken(html);
                html = await Post("https://www.fshare.vn/site/login", new Dictionary<string, string>()
                {
                    {token.Item1, token.Item2},
                    {"LoginForm[email]", this.username},
                    {"LoginForm[password]", this.Password},
                    {"LoginForm[rememberMe]", "1"},
                });
            }
            foreach (Cookie item in cookieContainer.GetAllCookies())
            {
                if (item.Name == "_identity-app")
                {

                    cookieContainer.Save(cookieFile);
                    return true;
                }
            }
            return false;
        }
        //Get link of 1 file

        public async Task<FShareFolderResponse> GetFilesInFolder(string url)
        {
            string urlToDownload = "";
            if(url.Contains("v3/files"))
            {
                urlToDownload = "https://www.fshare.vn/api" + url;
            }
            else
            {
                string splitUrl = url.Split('?')[0];
                var folderCode = splitUrl.Split("/").Last();
                urlToDownload = "https://www.fshare.vn/api/v3/files/folder?linkcode=" + folderCode + "&sort=type,name";
            }
            try
            {

                var client = new RestClient(urlToDownload);
                var request = new RestRequest(Method.GET);
                request.AddHeader("referer", url);
                request.AddHeader("authority", "www.fshare.vn");
                request.AddHeader("cache-control", "no-cache,no-cache");
                request.AddHeader("accept", "application/json, text/plain, */*");
                request.AddHeader("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                request.AddHeader("accept-language", "vi-VN,vi");
                request.AddHeader("accept-encoding", "gzip, deflate, br");
                request.AddHeader("pragma", "no-cache");
                var response = await client.ExecuteTaskAsync<FShareFolderResponse>(request);
                var result = FShareFolderResponse.FromJson(response.Content);
                Console.WriteLine($"Found {result.Items.Count} items ...", ConsoleColor.DarkGreen);
                if (result.Links.Next != null)
                {
                    var nextResults = await GetFilesInFolder(result.Links.Next);
                    if(nextResults != null && nextResults.Items != null)
                    result.Items.AddRange(nextResults.Items);
                }
                return result;
            }
            catch(Exception ex)
            {

            }
            return null;

        }

        public async Task<string> GetFileName(string url)
        {
            try
            {
                using (var wc = new HttpClient())
                {
                    var res = await wc.GetAsync(url);
                    var html = await res.Content.ReadAsStringAsync();
                    var match = Regex.Match(html, "id=\"limit-name\" title=\"([^\"]*)");
                    if (match != null && match.Groups.Count > 1)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
            catch (Exception ex)
            {

            }
           
            return null;
        }
        public bool IsFShareFile(string url)
        {
            var pattern = "https:\\/\\/www\\.fshare\\.vn\\/file\\/([A-Z0-9]*)";
            return Regex.IsMatch(url, pattern);
        }
        public bool IsFShareFolder(string url)
        {
            var pattern = "https:\\/\\/www\\.fshare\\.vn\\/folder\\/([A-Z0-9]*)";
            return Regex.IsMatch(url, pattern);
        }
        public async Task<FShareFile> GetFShareFileInfo(string url, bool userAccount= false)
        {
            string fshareApi = url.Replace("https://www.fshare.vn/file/", "https://www.fshare.vn/api/v3/files/folder?linkcode=");
            if (fshareApi == url) return null;
            try
            {
                string json = "";
                if (userAccount)
                {
                    json = await Get(fshareApi, true);
                }
                else
                {
                    using (var cl = new HttpClient())
                    {
                        json = await cl.GetStringAsync(fshareApi);
                    };
                }
                var response = FShareFolderResponse.FromJson(json);
                return response.Current;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetFShareFileInfo " + ex.Message);
                return null;
            }
        }
        public async Task<DownloadableItem> GetLink(string url)
        {
            if(!logged)
            {
                logged = await Login();
            }
            var html = await Get(url);
            var token = ExtractToken(html);
            string linkCOde = url.Split('?')[0].Split('/').Last();
            var json = await Post("https://www.fshare.vn/download/get", new Dictionary<string, string>()
            {
                {token.Item1, token.Item2},
                {"linkcode", linkCOde},
                {"withFcode5", "0"},
                {"fcode",string.Empty},
            });
            var response = FShareGetLinkResponse.FromJson(json);
            var fi = await GetFShareFileInfo(url);

            return new DownloadableItem()
            {
                Name = fi.Name,
                Size = fi.Size,
                Url = response.Url
            };

        }
    }
}
