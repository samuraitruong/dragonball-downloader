using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LinkFetcher.Models;

namespace LinkFetcher
{
    public class FShare
    {
        public FShare()
        {
        }
        public static async Task<FShareFolderResponse> GetFilesInFolder(string url)
        {
            string urlToDownload = "";
            if (url.Contains("v3/files"))
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

                using(var client = new HttpClient())
                {
                    var json = await client.GetStringAsync(urlToDownload);
                    var result = FShareFolderResponse.FromJson(json);
                    Console.WriteLine($"Found {result.Items.Count} items ...", ConsoleColor.DarkGreen);
                    if (result.Links.Next != null)
                    {
                        var nextResults = await GetFilesInFolder(result.Links.Next);
                        if (nextResults != null && nextResults.Items != null)
                            result.Items.AddRange(nextResults.Items);
                    }
                    return result;
                }
               
            }
            catch (Exception ex)
            {

            }
            return null;

        }

        public static async Task<FShareFileInfo> GetFShareFileInfo(string url)
        {
            string fshareApi = url.Replace("https://www.fshare.vn/file/", "https://www.fshare.vn/api/v3/files/folder?linkcode=");
            if (fshareApi == url) return null;
            try
            {
                using (var client = new HttpClient())
                {
                    var json = await client.GetStringAsync(fshareApi);
                    var response = FShareFileResponse.FromJson(json);
                    return response.CurrentFileInfo;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("GetFShareFileInfo " + ex.Message);
                return null;
            }
        }
    }
}
