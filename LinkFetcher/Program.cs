using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace LinkFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
             Run().Wait();
        }
        static async Task Run()
        {
            var dhvn = new HDVietNam("samuraitruong", "250538804");
            var logged = await dhvn.Login();
            Console.WriteLine("Logged successfull....");
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
    }
}
