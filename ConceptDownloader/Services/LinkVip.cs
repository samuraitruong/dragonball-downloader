using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ConceptDownloader.Models;
using RestSharp;

namespace ConceptDownloader.Services
{
    //https://linkvip.info
    public class LinkVip : ILinkFetcherService
    {
        public LinkVip()
        {
        }
        public async Task <DownloadableItem > GetLink(string url)
        {
            var encodedUrl = HttpUtility.UrlEncode(url);
            var client = new RestClient("https://linkvip.info/api/links?q=" + encodedUrl);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "application/json, text/plain, */*");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
            request.AddHeader("Accept-Language", "en-US,en;q=0.9,vi;q=0.8");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Pragma", "no-cache");
            var response = await client.ExecuteTaskAsync< LinksVipResponse>(request);
            var data = LinksVipResponse.FromJson(response.Content);
            return new DownloadableItem(data.Links.FirstOrDefault().Url)
            {
                Name = Path.GetFileName(data.Links.FirstOrDefault().Url)
            };
        }
    }
}
