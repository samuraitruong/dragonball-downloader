using System;
using System.Threading.Tasks;
using ConceptDownloader.Models;

namespace ConceptDownloader.Services
{
    public interface ILinkFetcherService
    {
        Task<DownloadableItem> GetLink(string url);
    }
}
