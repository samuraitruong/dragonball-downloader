using System;
using System.Threading.Tasks;
using ConceptDownloader.Models;
using RestSharp;

namespace ConceptDownloader.Services
{
    [Service("getlinkaz.com")]
    public class GetLinkAZ : ILinkFetcherService
    {
        private const int TIMEOUT = 100;

        public GetLinkAZ()
        {
        }
        public async Task<DownloadableItem> GetLink(string url)
        {
            try
            {
                var client = new RestClient("https://getlinkaz.com/get/GetLinkAzFshare/");
                client.Timeout = TIMEOUT * 1000;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Postman-Token", "1b59694d-9425-4fba-80fe-e3ff408d2511");
                request.AddHeader("referer", "https://getlinkaz.com/fshare/?link=" + url);
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
    }
}
