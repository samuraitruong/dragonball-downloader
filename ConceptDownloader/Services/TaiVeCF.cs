using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConceptDownloader.Models;
using RestSharp;

namespace ConceptDownloader.Services
{
    public class TaiVeCF: ILinkFetcherService
    {
        public TaiVeCF() 
        {
        }
        public async Task<DownloadableItem> GetLink(string url)
        {
            var client = new RestClient("http://taive.cf/get/submit");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json, text/plain, */*");
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
            request.AddHeader("Accept-Language", "en-US,en;q=0.9,vi;q=0.8");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Origin", "http://taive.cf");
            request.AddParameter("undefined", "{\"link\":\""+ url+"\",\"tvcode\":\"\"}", ParameterType.RequestBody);
            var response = await client.ExecuteTaskAsync<TaiveCfResponse>(request);
            var jw = response.Data.Link;
            string pattern = @"url=([^\\]*)";
            var match = Regex.Match(jw, pattern);
            if(match != null)
            {
                return new DownloadableItem(match.Groups[1].Value)
                {
                    Name = response.Data.Status.Split('|')[0]
                };
            }
            return null;
        }
    }
}
