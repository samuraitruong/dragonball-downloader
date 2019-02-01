using System.Collections.Generic;
using CommandLine;

namespace ConceptDownloader
{
    public class ApplicationArguments
    {
        [Option('o', "ouput", Default = "output", Required = false, HelpText = "Output folder to save download file, default: output")]
        public string Output { get; set; }
        [Option('t', "thread", Default = 40, Required = false, HelpText = "Number of threads use to download file, default: 40")]
        public int Thread { get; set; }
        [Option('r', "rename-to", HelpText = "Rename output file to ", Required = false)]
        public string RenameTo { get; set; }
        [Option('b', "buffer", Default = 2500000, HelpText = "Buffer size of chunk download, default 2.44MB ", Required = false)]
        public long Buffer { get; set; }
        [Option('u', "url", Required = true, HelpText = "Url to single file to download or page to scan in crawl-mode (-c true) ")]
        public string Url { get; set; }
        [Option('c', "crawl-mode", Default = false, Required = false, HelpText = "Download in crawl mode that collect links from html page and download em all")]
        public bool CrawlMode { get; set; }

        [Option("extract-link-mode", Default = false, Required = false, HelpText = "Extract link from webpage or gist file")]
        public bool ExtractLinkMode { get; set; }

        [Option('f', "filter", Required = false, HelpText = "Filter the link to download in crawl mode (ex :.mkv)")]
        public string Filter { get; set; }

        [Option("recursive", Required = false, HelpText = "In crawl mode, looks into the sub directory to get links too")]
        public bool Recursive { get; set; }
        [Option('a', "alternative-output", Required = false, HelpText = "Alternative Output folder to check for existing file. multiple support by comma")]
        public string AlternativeOutput { get; set; }

        [Option("mv", Required = false, HelpText = "Use MV or ren command to rename file instead")]
        public bool UseNativeRenameCommand { get; set; }

        [Option('e', "exclude", Required = false, HelpText = "Exclude all the path contain the word, multiple support separated by comma.")]
        public string Exclude { get; set; }
        [Option("download-all", Default = false, HelpText = "Download every file in folder with out parse to get by file size", Required = false)]
        public bool DownloadAll { get; internal set; }

        [Option("continue-from", HelpText = "Skips previos and continue from this if match file name", Required = false)]
        public string ContinueFrom { get; set; }
        [Option("link-service", HelpText = "The service to use to fetch fshare link (getlinkaz.com, linkvip.info, taive.cf, anlink.top)", Required = false)]
        public string GetLinkService { get; set; }

        [Option("link-service-username", HelpText = "The username of link service if reqired (fshare is required)", Required = false)]
        public string LinkServiceUsername { get; set; }

        [Option("link-service-password", HelpText = "The password of link service if required (fshare is required)", Required = false)]
        public string LinkServicePassword { get; set; }
        [Option("link-service-re-login", Default = false, HelpText = "For link service to relogin account instead of using cookies", Required = false)]
        public bool GetLinkServiceReLogin { get; set; }
        [Option('h', "history-file", Default = "download.log", HelpText = "File to keep track all the url and filename that program has successful download", Required = false)]
        public string HistoryFile { get; set; }
        public List<string> Excludes
        {
            get
            {
                return string.IsNullOrEmpty(this.Exclude) ? new List<string>() :
                new List<string>(this.Exclude.Split(";,".ToCharArray()));
            }
        }

        public List<string> AlternativeOutputs
        {
            get
            {
                return string.IsNullOrEmpty(this.AlternativeOutput) ? new List<string>() :
                new List<string>(this.AlternativeOutput.Split(";,".ToCharArray()));
            }
        }
        [Option("failed-chunk-limit", Default = 100, Required = false, HelpText = "Number of Failed chunk error that will stop download the file. ")]
        public int FailedChunkLimit { get; internal set; }
        [Option("retry", Default = 3, HelpText = "Number of retrying before go to next file or error", Required = false)]
        public int Retry { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<ApplicationArguments>(args)
                  .WithParsed<ApplicationArguments>(o =>
                  {
                      // read from file
                      SimpleDownloader.Run(o).Wait();
                  });
        }

    }
}

