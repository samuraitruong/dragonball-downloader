using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using LinkFetcher.Models;
using System.Collections.Concurrent;

namespace LinkFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            //Run().Wait();
            //RunFshareInfo();
            // MakeCSV();
            //HDVietNameTopic("").Wait();
            ExtractFshareFolder();
        }
        static void MakeCSV()
        {
            var rawJson = File.ReadAllText("in-lossless-links.json");
            var alltopic = JsonConvert.DeserializeObject<List<Item>>(rawJson);
            List<string> lines = new List<string>();
            lines.Add("Filename, URL, Size, FileSize, Topic, Source");
            foreach (var item in alltopic)
            {
                string filename = item.Url.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
                var fi = new FileInfo(Path.Combine("in-lossless-links-fs", filename + ".json"));
                if (!fi.Exists || fi.Length < 10) continue;

                var json = File.ReadAllText(fi.FullName);
                var movie = JsonConvert.DeserializeObject<MovieItem>(json);

                lines.AddRange(movie.Links.Select(x => $"{x.Name},{x.Url},{x.Size},{x.FileSize},{item.Name},{item.Url}"));
            }
            File.WriteAllLines("in-lossless.csv", lines);
        }
        static void ExtractFshareFolder()
        {
            ConcurrentBag<string> allFolder = new ConcurrentBag<string>();
            var rawJson = File.ReadAllText("in-lossless-links.json");
            var alltopic = JsonConvert.DeserializeObject<List<Item>>(rawJson);

            Parallel.ForEach(alltopic, new ParallelOptions() { MaxDegreeOfParallelism = 28 }, item =>
            {


                string filename = item.Url.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
                var fi = new FileInfo(Path.Combine("hdvietnam-in-lossless", filename + ".json"));
                if (!fi.Exists || fi.Length < 10) return;

                var json = File.ReadAllText(fi.FullName);
                var links = JsonConvert.DeserializeObject<List<string>>(json);
                links = links.Where(x => x.Contains("folder")).ToList();
                links.ForEach(x => allFolder.Add(x));
            });

            allFolder.WriteTo("in-lossless-folder.jon");
        }

        static void RunFshareInfo()
        {
            ConcurrentBag<string> allVNLossless = new ConcurrentBag<string>();
            var rawJson = File.ReadAllText("in-lossless-links.json");
            var alltopic = JsonConvert.DeserializeObject<List<Item>>(rawJson);

            Parallel.ForEach(alltopic, new ParallelOptions() { MaxDegreeOfParallelism = 28 }, item =>
            {


                string filename = item.Url.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
                var fi = new FileInfo(Path.Combine("hdvietnam-in-lossless", filename + ".json"));
                if (!fi.Exists || fi.Length < 10) return;
                Console.WriteLine(filename);

                var json = File.ReadAllText(fi.FullName);
                var links = JsonConvert.DeserializeObject<List<string>>(json);
                ConcurrentBag<FShareFileInfo> fshareInfos = new ConcurrentBag<FShareFileInfo>();
                Parallel.ForEach(links, url =>
                {

                    var info = FShare.GetFShareFileInfo(url).Result;
                    if (info != null)
                    {
                        fshareInfos.Add(info);
                    }
                });
                // save to target folder
                var movie = new MovieItem()
                {
                    Name = item.Name,
                    Url = item.Url,
                    Links = fshareInfos.Select(x => new Link()
                    {
                        Url = "https://www.fshare.vn/file/" + x.Linkcode,
                        Name = x.Name,
                        FileSize = x.Size,
                        Size = x.Size.ToHumandReadable()
                    }).ToList()
                };
                if (movie.Links.Count > 0)
                {
                    string output = Path.Combine("in-lossless-links-fs", filename + ".json");
                    movie.WriteTo(output);
                }

            });
        }
        static async Task Run()
        {
            var dhvn = new HDVietNam("samuraitruong", "250538804");
            var logged = await dhvn.Login();
            Console.WriteLine("Logged successfull....");

            //var alltopic = await dhvn.GetLinks("http://www.hdvietnam.com/forums/nhac-viet-nam.183/");
            ConcurrentBag<string> allVNLossless = new ConcurrentBag<string>();
            var rawJson = File.ReadAllText("vn-lossless-links.json");
            var alltopic = JsonConvert.DeserializeObject<List<Item>>(rawJson);
            //alltopic.WriteTo("vn-lossless-links.json");
            var possibleLinks = alltopic.Where(x => !x.Name.Contains("[MF]"));
            ConcurrentBag<Item> processCount = new ConcurrentBag<Item>(); ;
            //foreach (var item in possibleLinks)
            int totalLink = possibleLinks.Count();
            var color = Console.ForegroundColor;
            Parallel.ForEach(possibleLinks, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, item =>
             {
                 processCount.Add(item);
                 Console.WriteLine($"Process {(double)processCount.Count}/{totalLink} , Percent : {processCount.Count / totalLink: 0.00%}");
                 var outputPath = Path.Combine("hdvietnam-vn-lossless", Path.GetFileName(item.Url.TrimEnd('/') + ".json"));
                 if (File.Exists(outputPath)) return;
                 try
                 {
                     var listfshare = dhvn.GetFshareLink(item.Url).Result;
                     listfshare.Dump();

                     listfshare.WriteTo(outputPath);

                     Console.WriteLine("RESULT : " + item.Url + "\t Found: " + listfshare.Count);
                     listfshare.ForEach(x => allVNLossless.Add(x));
                     //allVNLossless.AddRange(listfshare);
                 }
                 catch (Exception ex)
                 {
                     Console.ForegroundColor = ConsoleColor.Red;
                     Console.Write(ex.Message + ex.StackTrace);
                     Console.WriteLine("ERROR: " + item.Url);
                     Console.ForegroundColor = color;
                 }
             });

            allVNLossless.WriteTo("allVNLossless.json");
            return;
            var fshareLinks = await dhvn.GetFshareLink("http://www.hdvietnam.com/threads/2000-cd-hai-ngoai-thuy-nga-asia-lang-van-giang-ngoc-hai-au-phuong-hoang-ndbd-multi.443591");
            fshareLinks.Dump();
            fshareLinks.WriteTo("haha.json");
            return;
            var l250 = await dhvn.GetLinksIntContent("http://www.hdvietnam.com/threads/collection-top-250-best-movies-of-imdb-720p-1080p-remux-250-phim-xep-hang-cao-nhat-theo-imdb.1406130/");
            l250.Dump();
            var all2050links = dhvn.GetMovieLinks(l250);
            //var links1 = await dhvn.GetMovieLinks("http://www.hdvietnam.com/threads/hanh-dong-chinh-kich-fight-club-1999-1080p-edt-bluray-dts-x264-d-z0n3-cau-lac-bo-chien-dau.1413883/");
            //links1.Dump();
            var csvData = all2050links.SelectMany(x => x.Links.Where(y => y.FileSize > 0).Select(link => $"{x.Name},{link.Url}, {link.Name}, {link.Size}")).ToList();
            File.WriteAllLines("IMDB250.csv", csvData);
            all2050links.WriteTo("imdb250.json");

            return;

            var t = new TaiPhimHD("samuraitruong@hotmail.com", "@Abc123$");
            var l = await t.Login();
            var links = await t.GetTopicPosts("https://taiphimhd.com/forums/phim-hoat-hinh.15/");
            links.Dump();
            var m = await t.GetMovieLinks("https://taiphimhd.com/threads/appleseed-trilogy-2004-2007-2014-1080p-bluray-aac-x264.3395/");
            m.Dump();
            return;
            var fs = new FSharePhim("samuraitruong", "@@ABcd123$$");
            //var info = await fs.GetFShareFileInfo("https://www.fshare.vn/file/38NA7WPKEYG1H4R");
            //info.Dump();
            //var result = await fs.Login();
            //var links = await fs.GetLinks("https://fsharephim.com/genre/lich-su/");

            //links.Dump();
            //return;
            //var item = await fs.FetchMovie("https://fsharephim.com/tv-shows/dreamworks-dragons/");
            // var item = await fs.FetchMovie("https://fsharephim.com/episodes/man-to-man-1x2/", false);
            //item.Dump();
            //return;
            //var list =  await fs.FetchCategory("https://fsharephim.com/genre/khoa-hoc-vien-tuong/", 50);
            //list.WriteTo("data/khoa-hoc-vien-tuong.json");
            var genres = await fs.GetGenreLinks();
            foreach (var genre in genres)
            {
                var file = "data/" + genre.Name + ".json";
                if (File.Exists(file)) continue;

                var list = await fs.FetchCategory(genre.Url);
                list.WriteTo(file);
            }
        }

        static async Task HDVietNameTopic(string url)
        {
            var dhvn = new HDVietNam("samuraitruong", "250538804");
            var logged = await dhvn.Login();
            Console.WriteLine("Logged successfull....");

            //var alltopic = await dhvn.GetLinks("http://www.hdvietnam.com/forums/nhac-khong-loi.185/");
            ConcurrentBag<string> allVNLossless = new ConcurrentBag<string>();
            var rawJson = File.ReadAllText("kl-lossless-links.json");
            var alltopic = JsonConvert.DeserializeObject<List<Item>>(rawJson);
            //alltopic.WriteTo("kl-lossless-links.json");
            var possibleLinks = alltopic.Where(x => !x.Name.Contains("[MF]"));
            ConcurrentBag<Item> processCount = new ConcurrentBag<Item>(); ;
            //foreach (var item in possibleLinks)
            int totalLink = possibleLinks.Count();
            var color = Console.ForegroundColor;
            Parallel.ForEach(possibleLinks, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, item =>
            {
                processCount.Add(item);
                Console.WriteLine($"Process {(double)processCount.Count}/{totalLink} , Percent : {processCount.Count / totalLink: 0.00%}");
                var outputPath = Path.Combine("hdvietnam-kl-lossless", Path.GetFileName(item.Url.TrimEnd('/') + ".json"));
                if (File.Exists(outputPath)) return;
                try
                {
                    var listfshare = dhvn.GetFshareLink(item.Url).Result;
                    listfshare.Dump();

                    listfshare.WriteTo(outputPath);

                    Console.WriteLine("RESULT : " + item.Url + "\t Found: " + listfshare.Count);
                    listfshare.ForEach(x => allVNLossless.Add(x));
                    //allVNLossless.AddRange(listfshare);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(ex.Message + ex.StackTrace);
                    Console.WriteLine("ERROR: " + item.Url);
                    Console.ForegroundColor = color;
                }
            });

            allVNLossless.WriteTo("allKLLossless.json");
            return;

        }

    }
}
