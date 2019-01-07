using System;
namespace ConceptDownloader
{
    public class DownloadException : Exception
    {
        public DownloadException(string message): base(message)
        {
        }
    }
}
