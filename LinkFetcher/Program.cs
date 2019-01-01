using System;
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

            var fs = new FShareFiml("samuraitruong", "@@ABcd123$$");
            //var info = await fs.GetFShareFileInfo("https://www.fshare.vn/file/38NA7WPKEYG1H4R");
            //info.Dump();
            var result = await fs.Login();
            /*var links = await fs.GetLinks("https://fsharephim.com/genre/hoat-hinh/");
            foreach (var item in links)
            {
                Console.WriteLine(item);
            }*/
//            var item = await fs.FetchMovie("https://fsharephim.com/movies/ghost-in-the-shell-2/");
//item.Dump();
           var list =  await fs.FetchCategory("https://fsharephim.com/genre/hoat-hinh/", 50);
            list.WriteTo("data/hoat-hinh.json");

        }
    }
}
