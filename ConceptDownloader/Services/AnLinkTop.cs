using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ConceptDownloader.Models;
using RestSharp;

namespace ConceptDownloader.Services
{
    public class AnLinkTop : ILinkFetcherService
    {
        public AnLinkTop()
        {
        }
        public async Task<DownloadableItem> GetLink(string url)
        {
            var encodedUrl = HttpUtility.UrlEncode(url);
            var client = new RestClient("https://getlink.anlink.top/fshare_api.php?id=64s2y21313p2w2e434v2t2b4r244w2y2w2");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Referer", "https://j2team.anlink.top/get-link-fshare/");
            request.AddHeader("Accept", "application/json, text/javascript, */*; q=0.01");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
            request.AddHeader("Accept-Language", "en-US,en;q=0.9,vi;q=0.8");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Origin", "https://j2team.anlink.top");
            request.AddParameter("undefined", "reget=1&url=" + encodedUrl, ParameterType.RequestBody);
            var response = await client.ExecuteTaskAsync<AnLinkTopResponse>(request);
            var data = AnLinkTopResponse.FromJson(response.Content);
            return new DownloadableItem(data.Url)
            {
                Name = Path.GetFileName(data.Url)
            };
        }
    }
}
