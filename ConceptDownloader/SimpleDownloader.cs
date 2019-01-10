using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Concurrent;
using Console = Colorful.Console;
using System.Drawing;
using System.Web;
using ConceptDownloader.Models;
using ConceptDownloader.Services;
using ConceptDownloader.Extensions;

namespace ConceptDownloader
{
    public class SimpleDownloader
    {
        private static Object consoleLocker = new Object();
        static int start = 0;
        static int end = 0;
        static DateTime lastStatusUpdate = DateTime.Now;
        public static ApplicationArguments options;
        static ConcurrentQueue<bool> waitingQueue = new ConcurrentQueue<bool>();

        static List<ILinkFetcherService> supportedServices;
        static async Task WriteWaiting()
        {
            while (waitingQueue.Count > 0)
            {
                Console.Write(".", Color.Gray);
                await Task.Delay(1000);
            }
        }
        static void WriteStatus(List<int> statuses, long blockSize, bool printLegend = false, bool toggleBlink = false)
        {
            lastStatusUpdate = DateTime.Now;
            lock (consoleLocker)
            {
                int col = Console.WindowWidth;
                int row = Console.WindowHeight;

                int blockPerRow = col / 3;
                int totalBlockCanDisplay = blockPerRow * (row - 6);

                if (end == 0) end = Math.Min(statuses.Count, totalBlockCanDisplay);
                try
                {

                    if (statuses.Count > totalBlockCanDisplay)
                    {
                        var half = totalBlockCanDisplay / 2;

                        var lastDownloadItem = statuses.LastIndexOf(1);
                        if (lastDownloadItem > (totalBlockCanDisplay + start - 2))
                        {
                            start = statuses.IndexOf(1);
                            if (start == -1)
                            {
                                start = statuses.LastIndexOf(2);
                            }
                            start = Math.Max(0, start);

                            end = Math.Min(statuses.Count, start + totalBlockCanDisplay);

                            if (end - start < totalBlockCanDisplay)
                            {
                                end = statuses.Count;
                                start = end - totalBlockCanDisplay;
                            }

                        }
                    }
                    Console.SetCursorPosition(0, 2);

                    int count = 0;
                    //Console.Title = $"{start} > xxx < {end} : total canbe {totalBlockCanDisplay}";
                    for (var i = start; i < end; i++)
                    {
                        Color color = Color.Black;
                        var item = statuses[i];
                        if (item == 1) color = toggleBlink ? Color.Yellow : Color.DarkOrange;
                        if (item == 0) color = Color.Gray;
                        if (item == 2) color = Color.DarkGreen;

                        Console.Write("██ ", color);
                        count++;
                        count = count % blockPerRow;

                        if (count == 0) Console.WriteLine("");

                    }

                }
                catch (Exception ex)
                {

                }
                finally
                {

                }
                if (printLegend)
                {
                    Console.WriteLine("\r\n");

                    Console.Write($"██ = {(double)blockSize / (1000 * 1024): 0.00} MB", Color.Gray);
                    Console.Write($" x{statuses.Count()}\t", Color.Magenta);

                    Console.Write($"██ : Downloading\t", Color.Yellow);

                    Console.WriteLine($"██ : Finished\t", Color.DarkGreen);
                }
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

            client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip }) { 
                Timeout = TimeSpan.FromSeconds(150)
            };

            return client;

        }
        static async Task BlinkBlock(Func<List<int>> getList, CancellationToken token)
        {
            bool flag = false;

            while (true)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(1500, token);
                if ((DateTime.Now - lastStatusUpdate).TotalMilliseconds > 1000)
                {
                    WriteStatus(getList(), 0, toggleBlink: flag);
                }

                flag = !flag;

            }
        }
        static bool CheckExistingFileInFolder(string filename, string folder, List<string> folders)
        {
            var folderToCheck = new List<string>() { folder };
            if (folders != null) folderToCheck.AddRange(folders);

            foreach (var target in folderToCheck)
            {
                var fullpath = Path.Combine(target, filename);
                if (File.Exists(fullpath)) return true;
            }
            return false;
        }

        static string DownloadFileWithMultipleThread(string url, string ouputFilename, string folder, List<string> checkFolders = null, int thread = 10, long chunkSize = 1024000)
        {

            string filename = Path.GetFileName(HttpUtility.UrlDecode(url));
            if (!string.IsNullOrEmpty(ouputFilename))
            {
                //need validate the url without file name
                filename = ouputFilename;
            }
            string output = Path.Combine(folder, filename);
            string logFile = output + ".log";
            if (CheckExistingFileInFolder(filename, folder, checkFolders))
            {
                Console.WriteLine($"[{filename}] - Has been downloaded, ignore download this file, using -f or --force to re-download this file.", Color.DimGray);
                return output;
            }
            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception ex) { }
            }

            List<int> downloadedChunks = new List<int>();
            start = 0;
            end = 0;
            double resumeBytes = 0;

            if (File.Exists(logFile))
            {
                var lines = File.ReadAllLines(logFile);
                if (lines.Length == 0 || Convert.ToInt32(lines[0]) != chunkSize)
                {
                    File.Delete(logFile);
                    File.WriteAllText(logFile, chunkSize.ToString());
                }
                else
                {
                    downloadedChunks.AddRange(lines.Skip(1).Select(x => Convert.ToInt32(x)));
                    resumeBytes = downloadedChunks.Count * chunkSize;
                }
            }
            else
            {
                File.WriteAllText(logFile, chunkSize.ToString());
            }


            int mb = 1024 * 1000;

            int numberOfChunks = 0;
            double totalSizeBytes = 0;
            double downloadedBytes = 0;

            var client = GetClient();

            var request = new HttpRequestMessage(HttpMethod.Head, new Uri(url));

            var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;

            IEnumerable<string> values;
            if (response.Content.Headers.TryGetValues("Content-Length", out values))
            {
                totalSizeBytes = Convert.ToInt64(values.First());

                lock (consoleLocker)
                {
                    Console.Clear();
                    var trimmedTitle = filename.Substring(0, Math.Min(Console.WindowWidth - 30, filename.Length));
                    Console.WriteLine($"Downloading {trimmedTitle} | Size {totalSizeBytes / mb:0.00} MB ", Color.DarkMagenta);
                }
                numberOfChunks = (int)(totalSizeBytes / chunkSize) + (((long)totalSizeBytes % chunkSize != 0) ? 1 : 0);
            }

            httpClients.Enqueue(client);
            ConcurrentBag<int> failedChunks = new ConcurrentBag<int>();

            //Initial empty file 
            var locker = new Object();
            string tempFile = output + ".chunks";
            CancellationTokenSource cts = new CancellationTokenSource();

            using (var fs = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                fs.SetLength((long)totalSizeBytes);

                ConcurrentDictionary<int, int> chunks = new ConcurrentDictionary<int, int>();
                for (int i = 1; i <= numberOfChunks; i++)
                {
                    chunks.TryAdd(i, downloadedChunks.Contains(i) ? 2 : 0);
                }
                var list = chunks.Keys.Select(x => chunks[x]).ToList();
                WriteStatus(list, chunkSize, printLegend: true);
                var startTime = DateTime.Now;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BlinkBlock(() => chunks.Keys.Select(x => chunks[x]).ToList(), cts.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                ThreadPool.SetMinThreads(thread, thread);
                var lResult = Parallel.ForEach(Enumerable.Range(1, numberOfChunks ).Where(x => !downloadedChunks.Contains(x)),
                 new ParallelOptions() { MaxDegreeOfParallelism = thread }, (s, state, index) =>
                 {
                     string chunkFileName = output + "." + s;
                     long chunkStart = (s - 1) * chunkSize;
                     long chunkEnd = Math.Min(chunkStart + chunkSize - 1, (long)totalSizeBytes);
                     lock (locker)
                     {
                         threadsCount++;
                         chunks.TryUpdate(s, 1, 0);
                         list = chunks.Keys.Select(x => chunks[x]).ToList();
                         UpdateConsoleTitle();
                         WriteStatus(list, chunkSize);
                     }

                     var chunkDownload = DownloadChunk(url, chunkStart, chunkEnd, chunkFileName).Result;

                     if(chunkDownload.Length < chunkEnd - chunkStart)
                     {
                         // Console.WriteLine($"Inconsistent data expected : {chunkEnd - chunkStart} but only got {chunkDownload.Length}");
                         failedChunks.Add(s);
                         if (failedChunks.Count > options.FailedChunkLimit)
                             state.Break();
                         else
                         {
                             lock (locker)
                             {
                                 threadsCount--;
                                 return;
                             }
                         };
                     }
                     lock (locker)
                     {
                         threadsCount--;
                         downloadedBytes += chunkDownload.Length;
                         var ts = DateTime.Now - startTime;
                         var kbs = (downloadedBytes / 1000) / ts.TotalSeconds;
                         var eta = (totalSizeBytes - downloadedBytes - resumeBytes) / 1024 / kbs;
                         double etaHour = Math.Floor(eta / 3600);
                         double etaMin = Math.Floor((eta - etaHour * 3600) / 60);
                         double etaSec = eta % 60;
                         lock (consoleLocker)
                         {
                             Console.SetCursorPosition(0, 1);

                             Console.WriteLine($"#{s:000} | Received: {(downloadedBytes + resumeBytes) / mb:0.00} MB - {(downloadedBytes + resumeBytes) / totalSizeBytes:P2} | Speed : {kbs:0.00} KB/s | ETA: {etaHour:00}:{etaMin:00}:{etaSec:00}");
                         }
                         fs.Seek(chunkStart, SeekOrigin.Begin);
                         fs.Write(chunkDownload, 0, chunkDownload.Length);
                         chunks.TryUpdate(s, 2, 1);
                         list = chunks.Keys.Select(x => chunks[x]).ToList();
                         WriteStatus(list, chunkSize);
                         UpdateConsoleTitle();
                         File.AppendAllText(logFile, "\r\n" + s);
                     }
                 });

                if(!lResult.IsCompleted)
                {
                    throw new DownloadException("File download not completed");
                }
            }
            cts.Cancel();

            Console.WriteLine("\r\nFile Download completed.");

            if (failedChunks.Count == 0)
            {
                if (options.UseNativeRenameCommand)
                {
                    var cmd = $"mv {tempFile} {output}";
                    cmd.Bash();
                    return output;
                }

                //Task.Run(() =>
                //{
                File.Move(tempFile, output);
                File.Delete(logFile);
                //});
            }
            return output;

        }

        private static async Task<byte[]> DownloadChunk(string url, long chunkStart, long chunkEnd, string chunkFileName, int retry = 5)
        {
            byte[] data = null;
            var client = GetClient();

            try
            {

                var request = new HttpRequestMessage { RequestUri = new Uri(url) };
                request.Headers.Range = new RangeHeaderValue(chunkStart, chunkEnd);

                var response = await client.SendAsync(request);
                data = await response.Content.ReadAsByteArrayAsync();

                if (data.Length < chunkEnd - chunkStart)
                {
                    throw new Exception("Data checked not pass, retrying...");
                }
            }
            catch (Exception ex)
            {
                if (retry > 0)
                    return await DownloadChunk(url, chunkStart, chunkEnd, chunkFileName, retry - 1);

                throw ex;
            }
            finally
            {
                httpClients.Enqueue(client);
            }

            return data;//chunkFileName;
        }
        #region Download single file with 1 thread, not being used
        static async Task<String> DownloadFile(string url, string folder)
        {

            DateTime start = DateTime.Now;

            string filename = Path.GetFileName(HttpUtility.UrlDecode(url));
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
                Console.WriteLine($"[{filename}] Downloading: {recieved} MB/{totalMB:0.00} MB | ({(double)e.BytesReceived / totalSizeBytes:P}) | Speed : {speed / 1024:0.00} kb/s", Color.DarkGray);
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
        #endregion
        private static int totalItems = 0;
        private static int indexCount = 0;
        private static int threadsCount = 0;
        private static string currentUrl = "";
        private static void UpdateConsoleTitle()
        {
            string title = $"{indexCount} | {totalItems} | {threadsCount} | {currentUrl} ==> {options.Output}";
            Console.Title = title;
        }
        private static DownloadableItem GetDownloadableItem(string url, string preferServiceName = null)
        {
            foreach (var service in supportedServices)
            {
                if(!string.IsNullOrEmpty(preferServiceName))
                {
                    var attributes = service.GetType().GetCustomAttributes(true).Cast<ServiceAttribute>().ToList();
                    if (attributes.Count > 0 && attributes[0].Name != preferServiceName) continue;
                }
                var output = service.GetLink(url).Result;
                if (output != null)
                {
                    output.Name = HttpUtility.UrlDecode(output.Name);
                    return output;
                }
            }
            return null;
        }
        public static async Task Run(ApplicationArguments inputOptions)
        {
            totalItems = 0;
            indexCount = 0;
            threadsCount = 0;
            options = inputOptions;
            List<DownloadableItem> urlsToDownload = new List<DownloadableItem>();
            //TODO refactore to better code because it gonna messup if more site .
            var fs = new Fshare(options.LinkServiceUsername, options.LinkServicePassword);
            if(options.GetLinkServiceReLogin)
            {
                fs.Login(true).Wait();
            }
            supportedServices = new List<ILinkFetcherService>()
            {
                new GetLinkAZ(),
                new LinkVip(),
                new TaiVeCF(),
                new AnLinkTop(),
                fs
            };

            //fshare
            if (fs.IsFShareFolder(options.Url))
            {
                Console.WriteLine("Wating to read fshare folder content: " + options.Url);
                var folderContents = await fs.GetFilesInFolder(options.Url);
                if (folderContents != null)
                {
                    var listItem = folderContents.Items.Select(x => new DownloadableItem(x.Url)
                    {
                        Name = x.Name,
                        Size = x.Size.Value,
                        ShortName = x.Name.ToMovieShortName()
                    }).ToList();
                    if (!options.DownloadAll)
                    {
                        var filtered = listItem.GroupBy(x => x.ShortName, (key, grouped) =>
                        {

                            return grouped.OrderByDescending(x => x.Size)
                                .Where(x => !options.Excludes.Exists(e => x.Name.ToLower().Contains(e.ToLower(), StringComparison.CurrentCulture)))
                                .FirstOrDefault();
                        });


                        urlsToDownload.AddRange(filtered.Where(x => x != null));
                    }
                    else
                    {
                        urlsToDownload.AddRange(listItem);
                    }
                }
            }
            var crawler = new SimpleCrawler(options);
            if (File.Exists(options.Url))
            {
                var urlsFile = File.ReadAllLines(options.Url).ToList();

                urlsToDownload.AddRange(urlsFile.Select(x => new DownloadableItem(x)));
            }
            if (!String.IsNullOrEmpty(options.Url) && options.ExtractLinkMode)
            {
                Console.WriteLine("Wating to fetch content from : " + options.Url);
                var urls = await crawler.ExtractLinks(options.Url);
                urlsToDownload.AddRange(urls);
            }

            if (!String.IsNullOrEmpty(options.Url) && options.CrawlMode)
            {
                Console.WriteLine("Wating to fetch content....");
                var urls = await crawler.GetLinks(options.Url, options.Recursive);
                urlsToDownload.AddRange(urls);
            }
            if (urlsToDownload.Count == 0)
            {
                urlsToDownload.Add(new DownloadableItem(options.Url));
            }

            totalItems = urlsToDownload.Count;
            bool continueAlready = false;
            foreach (var item in urlsToDownload)
            {
                currentUrl = item.Url;
                indexCount++;
                var downloadableItem = item;
                if (!continueAlready &&
                !string.IsNullOrEmpty(options.ContinueFrom) &&
                    !item.Url.ToLower().Contains(options.ContinueFrom.ToLower()))
                {
                    continue;
                }
                continueAlready = true;
                if (!string.IsNullOrEmpty(options.Filter) &&
                !item.Name.ToLower().Contains(options.Filter.ToLower())) continue;

                if (options.Excludes.Exists(x => item.Url.ToLower().Contains(x.ToLower(), StringComparison.CurrentCulture)) || 
                (!string.IsNullOrEmpty(item.Name) && options.Excludes.Exists(x => item.Name.ToLower().Contains(x.ToLower(), StringComparison.CurrentCulture))))
                {
                    continue;
                }
                Console.Clear();
                if (fs.IsFShareFile(item.Url))
                {
                    if (string.IsNullOrEmpty(item.Name))
                    {
                        item.Name = (await fs.GetFShareFileInfo(item.Url)).Name;
                    }
                    //check existing here 
                    var filename = Path.Combine(options.Output, item.Name);
                    if (File.Exists(filename)) continue;
                    waitingQueue.Enqueue(true);
                    Console.WriteLine("Fetching fshare file information: " + item.Url, Color.DarkMagenta);
                    Console.Write("Please wait......", Color.DarkOrange);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    WriteWaiting();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    var fi = GetDownloadableItem(downloadableItem.Url, options.GetLinkService);
                    waitingQueue.Clear();

                    if (fi == null) continue;
                    downloadableItem = fi;
                    downloadableItem.Name = downloadableItem.Name != null ? downloadableItem.Name : item.Name;
                }
                UpdateConsoleTitle();
                Console.Clear();
                int retry = 0;
                bool hasError = false;
                do
                {
                    try
                    {
                        hasError = false;
                        if (!string.IsNullOrEmpty(options.RenameTo)) downloadableItem.Name = options.RenameTo;
                        // Console.WriteLine(item.Url);
                        DownloadFileWithMultipleThread(downloadableItem.Url, downloadableItem.Name, options.Output, options.AlternativeOutputs, options.Thread, options.Buffer);
                    }
                    catch (DownloadException dle)
                    {
                        hasError = true;
                        Console.WriteLine(dle.Message);
                    }
                } while (retry < options.Retry && hasError);
            }
        }
    }
}