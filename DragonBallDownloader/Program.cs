using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Fclp;
using System.Linq;
using System.Threading;

namespace DragonBallDownloader
{
    public class ApplicationArguments
    {
        public string Series { get; set; }
        public string Output { get; set; }
        public int Thread { get; set; }
        public string RenameTo { get; set; }

        public static FluentCommandLineParser<ApplicationArguments> Setup()
        {
            var p = new FluentCommandLineParser<ApplicationArguments>();

            // specify which property the value will be assigned too.
            p.Setup(arg => arg.Output)
             .As('o', "output") // define the short and long option name
             .WithDescription("Set output folder, default to current folder /output")
             .SetDefault("output"); // using the standard fluent Api to declare this Option as required.

            p.Setup(arg => arg.Thread)
             .As('t', "threads")
             .WithDescription("Set number of concurency chapter you want to download. default is 2")
             .SetDefault(2);

            p.Setup(arg => arg.Series)
             .As('s', "series")
             .WithDescription("5 available series are : classic, dragonballz, dragonballgt, dragonballkai, dragonballsuper")
             .SetDefault("classic"); // use

            p.Setup(arg => arg.RenameTo)
            .As('r', "rename-to")
             .SetDefault("{0:00}.mp4"); // use
            p.SetupHelp("?", "help")
          .Callback(text => Console.WriteLine(text));
                return p;
            }
    }
    struct Series{
        public string Url { get; set; }
        public int Chap { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var parser = ApplicationArguments.Setup();
            parser.HelpOption.ShowHelp((parser.Options));

            var cmds = parser.Parse(args);

            if(cmds.HasErrors) {
                Console.WriteLine("Please proide ", cmds.ErrorText);
            }

            var options = parser.Object;
            var sources = new Dictionary<string, Series>() {
                {"classic",new Series() {Url= "http://saiyanwatch.com/videos/dragon-ball/{0:000}.mp4", Chap = 149}},
                {"dragonballz",new Series() {Url= "http://saiyanwatch.com/videos/dragon-ball-z/{0:000}.mp4", Chap = 290}},
                {"dragonballgt",new Series() {Url= "http://saiyanwatch.com/videos/dragon-ball-gt/{0:000}.mp4", Chap = 64}},
                {"dragonballkai",new Series() {Url= "http://saiyanwatch.com/videos/dragon-ball-kai/{0:000}.mp4", Chap = 167}},
                {"dragonballsuper",new Series() {Url= "http://saiyanwatch.com/videos/dragon-ball-super-sub/{0:000}.mp4", Chap = 113}}
            };


            var series = sources[options.Series];

            Console.WriteLine("DRAGON BALL MOVIE DOWNLOADER");
            Console.WriteLine("Series: {0}", options.Series );
            Console.WriteLine("Total Chapter: {0}", series.Chap);

            var loops = Enumerable.Range(1, series.Chap);

            Parallel.ForEach(loops, new ParallelOptions() {MaxDegreeOfParallelism = options.Thread}, (s, state, index) => {
                var url = string.Format(series.Url, s);
                var output = DownloadFile(url, options.Output).Result;

            });

            //var output = DownloadFile(url, "output").Result;

            Console.WriteLine("Cool!!!, All file have been downloaded. please check output folder");

        }
        static async Task<String> DownloadFile(string url, string folder) {

            DateTime start = DateTime.Now;

            string filename = Path.GetFileName(url);
            string output = Path.Combine(folder, filename);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }



            bool finish = false;
            WebClient webClient = new WebClient();

            var stream = webClient.OpenRead(url);

            long totalSizeBytes = Convert.ToInt64(webClient.ResponseHeaders["Content-Length"]);
            double totalMB =  (double)totalSizeBytes / (1024 * 1000);
                
            if (File.Exists(output))
            {

                var fi = new FileInfo(output);
                if (fi.Length == totalSizeBytes) {
                    Console.WriteLine($"[{filename}] - Has been downloaded, ignore download this file, using -f or --force to re-download this file.");
                    return output;
                }
            }

            Console.WriteLine($"[{filename}] - FileSize: {totalMB:0.00} MB");
            var position = Console.CursorTop - 1;
            var lastUpdate = DateTime.Now.AddMinutes((-1));

            webClient.DownloadProgressChanged += (sender, e) => {
                if (totalSizeBytes == e.BytesReceived)
                {
                    finish = true;
                }
                else
                if ((DateTime.Now - lastUpdate).TotalSeconds < 3) return;
                
                var ts = DateTime.Now - start;
                var speed = e.BytesReceived / ts.TotalSeconds;
                double recieved = Math.Round((double)e.BytesReceived / 1024 / 1000,2);
                Console.SetCursorPosition(0,position);
                Console.WriteLine($"[{filename}] Downloading: {recieved} MB/{totalMB:0.00} MB | ({(double)e.BytesReceived / totalSizeBytes:P}) | Speed : {speed/1024:0.00} kb/s");
                lastUpdate = DateTime.Now;

            };

            //webClient.DownloadFileAsync();
            webClient.DownloadDataCompleted += (sender, e) => {
                Console.WriteLine($"[{filename}] Download completed");
                finish = true;
            };
            Console.WriteLine($"[{filename}] Start Download, Output: {output}");
            Console.WriteLine("");
            position = Console.CursorTop - 1;
            try{
                webClient.DownloadFileAsync(new Uri(url), output);
                                
            }
            catch(Exception ex) {
                //log, output clean ....
                Console.WriteLine(ex.Message);
            }
            finally{
                //finish = true;
            }

            while (!finish) {
                await Task.Delay(5000);
            }
            return output;

        }
    }
}
