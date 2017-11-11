using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Fclp;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Concurrent;

namespace DragonBallDownloader
{
    public class ApplicationArguments
    {
        public string Series { get; set; }
        public string Output { get; set; }
        public int Thread { get; set; }
        public string RenameTo { get; set; }
        public long Buffer { get; set; }

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
             .SetDefault(16);

            p.Setup(arg => arg.Series)
             .As('s', "series")
             .WithDescription("5 available series are : classic, dragonballz, dragonballgt, dragonballkai, dragonballsuper")
             .SetDefault("classic"); // use

            p.Setup(arg => arg.Buffer)
            .As('b', "buffer") // define the short and long option name
            .WithDescription("Set the size of buffer")
            .SetDefault(1000 * 1024);

            p.Setup(arg => arg.RenameTo)
            .As('r', "rename-to")
             .SetDefault("{0:00}.mp4"); // use
            p.SetupHelp("?", "help")
          .Callback(text => Console.WriteLine(text));
            return p;
        }
    }
    struct Series
    {
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

            if (cmds.HasErrors)
            {
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
            Console.WriteLine("Series: {0}", options.Series);
            Console.WriteLine("Total Chapter: {0}", series.Chap);

            var loops = Enumerable.Range(1, series.Chap);

            Parallel.ForEach(loops, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, (s, state, index) =>
            {
                var url = string.Format(series.Url, s);
                //var output = DownloadFile(url, options.Output).Result;

                DownloadFileWithMultipleThread(url, options.Output, options.Thread, options.Buffer);
            });

            Console.WriteLine("Cool!!!, All file have been downloaded. please check output folder");

        }
        private static Object consoleLocker = new Object();
        static void WriteStatus(List<int> statuses, long blockSize ,ConsoleColor color = ConsoleColor.DarkGreen, bool printLegend = false )
        {
            lock (consoleLocker)
            {
                Console.SetCursorPosition(0, 2);
                var old = Console.ForegroundColor;
                int count = 0;
                foreach (var item in statuses)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    if (item == 1) Console.ForegroundColor = ConsoleColor.DarkYellow;
                    if (item == 2) Console.ForegroundColor = color;
                    Console.Write("██ ");
                    count++;
                    count = count % 25;

                    if (count == 0) Console.WriteLine("");

                }
                if (printLegend)
                {
                    Console.WriteLine("\r\n\r\n");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"██ : Block of {blockSize / (1000 * 1024): 0.00} MB");

                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"██ : Downloading");


                    Console.ForegroundColor = color;
                    Console.WriteLine($"██ : Finish");

                    Console.ForegroundColor =  ConsoleColor.DarkMagenta;
                    Console.WriteLine($"Total chunks: {statuses.Count()}");

                    Console.WriteLine("\r\n\r\n");
                }
                Console.ForegroundColor = old;

            }

        }
        static ConcurrentQueue<HttpClient> httpClients = new ConcurrentQueue<HttpClient>();
        static HttpClient GetClient()
        {
            HttpClient client = null;

            if (httpClients.TryDequeue(out client))
            {
                return client;
            }

            client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });

            return client;

        }
        static string DownloadFileWithMultipleThread(string url, string folder, int thread = 10, long chunkSize = 1024000)
        {
            string filename = Path.GetFileName(url);
            string output = Path.Combine(folder, filename);
            if (File.Exists(output)) {
                Console.WriteLine($"[{filename}] - Has been downloaded, ignore download this file, using -f or --force to re-download this file.");
                return output;
            }
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            int mb = 1024 * 1000;

            int numberOfChunks = 0;
            double totalSizeBytes = 0;
            double downloadedBytes = 0;

            var client = GetClient();

            var request = new HttpRequestMessage(HttpMethod.Head, new Uri(url));

            var response = client.SendAsync(request).Result;

            //using (var webClient = new WebClient())
            //{
            //  var stream = webClient.OpenRead(url);
            IEnumerable<string> values;
            if (response.Content.Headers.TryGetValues("Content-Length", out values))
            {
                totalSizeBytes = Convert.ToInt64(values.First());
                /*if (File.Exists(output))
                {

                    var fi = new FileInfo(output);
                    if (fi.Length == (long)totalSizeBytes)
                    {
                        Console.WriteLine($"[{filename}] - Has been downloaded, ignore download this file, using -f or --force to re-download this file.");
                        return output;
                    }
                }*/
                lock (consoleLocker)
                {
                    Console.Clear();

                    Console.WriteLine($"Downloading {filename} | Size {totalSizeBytes / mb:0.00} MB ");
                }
                numberOfChunks = (int)(totalSizeBytes / chunkSize) + (((long)totalSizeBytes % chunkSize != 0) ? 1 : 0);
            }

            httpClients.Enqueue(client);

            //Initial empty file 
            var locker = new Object();
            string tempFile = output + ".chunks";

            using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.SetLength((long)totalSizeBytes);


                ConcurrentDictionary<int, int> chunks = new ConcurrentDictionary<int, int>();
                for (int i = 1; i <= numberOfChunks; i++)
                {
                    chunks.TryAdd(i, 0);
                }
                var list = chunks.Keys.Select(x => chunks[x]).ToList();
                WriteStatus(list, chunkSize, printLegend:true);
                var startTime = DateTime.Now;


                Parallel.ForEach(Enumerable.Range(1, numberOfChunks), new ParallelOptions() { MaxDegreeOfParallelism = thread }, (s, state, index) =>
                {
                    string chunkFileName = output + "." + s;
                    long chunkStart = (s - 1) * chunkSize;
                    long chunkEnd = Math.Min(chunkStart + chunkSize - 1, (long)totalSizeBytes);
                    lock (locker)
                    {
                        chunks.TryUpdate(s, 1, 0);
                        list = chunks.Keys.Select(x => chunks[x]).ToList();
                        WriteStatus(list, chunkSize);
                    }

                    var chunkDownload = DownloadChunk(url, chunkStart, chunkEnd, chunkFileName).Result;

                    lock (locker)
                    {
                        downloadedBytes += chunkDownload.Length;
                        var ts = DateTime.Now - startTime;
                        var kbs = (downloadedBytes / 1000) / ts.TotalSeconds;
                        var eta = (totalSizeBytes - downloadedBytes) / 1024 / kbs;
                        lock (consoleLocker)
                        {
                            Console.SetCursorPosition(0, 1);

                            Console.WriteLine($"#{s} | Received: {downloadedBytes / mb:0.00} MB - {downloadedBytes / totalSizeBytes:P2} | Speed : {kbs:0.00} kbs | ETA: {eta / 3600:00}:{eta / 60:00}:{eta % 60:00}");
                        }
                        fs.Seek(chunkStart, SeekOrigin.Begin);
                        fs.Write(chunkDownload, 0, chunkDownload.Length);
                        chunks.TryUpdate(s, 2, 1);
                        list = chunks.Keys.Select(x => chunks[x]).ToList();
                        WriteStatus(list, chunkSize);

                    }

                });
            }

            Console.WriteLine("\r\nFile Download completed.");
            File.Move(tempFile, output);
            return output;

        }

        private static async Task<byte[]> DownloadChunk(string url, long chunkStart, long chunkEnd, string chunkFileName, int retry = 3)
        {
            byte[] data = null;
            var client = GetClient();

            try
            {

                var request = new HttpRequestMessage { RequestUri = new Uri(url) };
                request.Headers.Range = new RangeHeaderValue(chunkStart, chunkEnd);

                var response = await client.SendAsync(request);
                data = await response.Content.ReadAsByteArrayAsync();
                httpClients.Enqueue(client);
            }
            catch (Exception ex)
            {
                if (retry > 0)
                    return await DownloadChunk(url, chunkStart, chunkEnd, chunkFileName, retry - 1);

                throw ex;
            }
            /*using (var fs = File.OpenWrite(chunkFileName))
            {
                
                fs.Write(data, 0, data.Length);
            }*/
            //Console.SetCursorPosition(0, 1);
            //Console.WriteLine("Chunk #" + chunkFileName  + "[" + data.Length.ToString()+"]");
            return data;//chunkFileName;
        }

        static async Task<String> DownloadFile(string url, string folder)
        {

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
            double totalMB = (double)totalSizeBytes / (1024 * 1000);

            if (File.Exists(output))
            {

                var fi = new FileInfo(output);
                if (fi.Length == totalSizeBytes)
                {
                    Console.WriteLine($"[{filename}] - Has been downloaded, ignore download this file, using -f or --force to re-download this file.");
                    return output;
                }
            }

            Console.WriteLine($"[{filename}] - FileSize: {totalMB:0.00} MB");
            var position = Console.CursorTop - 1;
            var lastUpdate = DateTime.Now.AddMinutes((-1));

            webClient.DownloadProgressChanged += (sender, e) =>
            {
                if (totalSizeBytes == e.BytesReceived)
                {
                    finish = true;
                }
                else
                if ((DateTime.Now - lastUpdate).TotalSeconds < 3) return;

                var ts = DateTime.Now - start;
                var speed = e.BytesReceived / ts.TotalSeconds;
                double recieved = Math.Round((double)e.BytesReceived / 1024 / 1000, 2);
                Console.SetCursorPosition(0, position);
                Console.WriteLine($"[{filename}] Downloading: {recieved} MB/{totalMB:0.00} MB | ({(double)e.BytesReceived / totalSizeBytes:P}) | Speed : {speed / 1024:0.00} kb/s");
                lastUpdate = DateTime.Now;

            };

            //webClient.DownloadFileAsync();
            webClient.DownloadDataCompleted += (sender, e) =>
            {
                Console.WriteLine($"[{filename}] Download completed");
                finish = true;
            };
            Console.WriteLine($"[{filename}] Start Download, Output: {output}");
            Console.WriteLine("");
            position = Console.CursorTop - 1;
            try
            {
                webClient.DownloadFileAsync(new Uri(url), output);

            }
            catch (Exception ex)
            {
                //log, output clean ....
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //finish = true;
            }

            while (!finish)
            {
                await Task.Delay(5000);
            }
            return output;

        }
    }
}

