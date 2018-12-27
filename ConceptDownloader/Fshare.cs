using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConceptDownloader.Models;
using RestSharp;

namespace ConceptDownloader
{
    public class Fshare
    {
        private const int TIMEOUT = 100;
        public Fshare()
        {
        }
        //Get link of 1 file
        public async Task<DownloadableItem> GetLink(string url)
        {
            try
            {
                var client = new RestClient("https://getlinkaz.com/get/GetLinkAzFshare/");
                client.Timeout = TIMEOUT * 1000;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Postman-Token", "1b59694d-9425-4fba-80fe-e3ff408d2511");
                request.AddHeader("referer", "https://getlinkaz.com/fshare/?link=" +url);
                request.AddHeader("authority", "getlinkaz.com");
                request.AddHeader("cache-control", "no-cache,no-cache");
                request.AddHeader("accept", "application/json, text/javascript, */*; q=0.01");
                request.AddHeader("content-type", "application/x-www-form-urlencoded; charset=UTF-8");
                request.AddHeader("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                request.AddHeader("pragma", "no-cache");
                //request.AddHeader("cookie", "__cfduid=d11ff76823c3efe1f33fe5bec9ad818021545814539; _ga=GA1.2.1027637366.1545814542; _gid=GA1.2.2039428728.1545814542; PHPSESSID=nivomg48ieslgdu3mvsblg8fs6; __asc=04fb7e2b167e9baa327965a97e5; __auc=04fb7e2b167e9baa327965a97e5");
                request.AddHeader("x-requested-with", "XMLHttpRequest");
                request.AddHeader("accept-language", "en-US,en;q=0.9,vi;q=0.8");
                request.AddHeader("origin", "https://getlinkaz.com");
                request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", $"link={url}&pass=getlinkaz.comundefined&hash=uk962FFYt.bonVtCyiC6Z.SiTeBurlyZ&captcha=", ParameterType.RequestBody);
                var response = await client.ExecuteTaskAsync(request);
                var data = GetlinkAzModel.FromJson(response.Content);
                return new DownloadableItem()
                {
                    Name = data.Filename,
                    //Size = response.Data.Size,
                    Url = data.Linkvip
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error get link " + url);
            }
            return null;
        }

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
                if (result.Links.Next != null)
                {
                    var nextResults = await GetFilesInFolder(result.Links.Next);
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

    }
}
