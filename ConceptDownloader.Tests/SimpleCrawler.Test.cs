using System;
using System.Linq;
using System.Threading.Tasks;
using ConceptDownloader;
using Xunit;

namespace ConceptDownloader.Tests
{
    public class SimpleCrawlerTest
    {
        private SimpleCrawler crawler;
        public SimpleCrawlerTest()
        {
            crawler = new SimpleCrawler();

        }
        [Fact]
        public async Task GetLinks_NoRecursive()
        {
            var list = await this.crawler.GetLinks("http://dl8.heyserver.in/film/2018-11/");
            Assert.NotEmpty(list);

            Assert.Equal("1987.When.the.Day.Comes.2017.1080p.BluRay.x264.6CH-Pahe.mkv", list[0].Name);
            Assert.Equal("http://dl8.heyserver.in/film/2018-11/1987.When.the.Day.Comes.2017.1080p.BluRay.x264.6CH-Pahe.mkv", list[0].Url);
            Assert.Equal(2263285833, list[0].Size);
        }
        [Fact]
        public async Task GetLinks_Recursive_Return_AllPage()
        {
            var list = await this.crawler.GetLinks("http://dl8.heyserver.in/film/", true);
            Assert.NotEmpty(list);
            var item = list.FirstOrDefault(x => x.Name == "100.Streets.2016.1080p.BluRay.6CH.x264.HeyDL.mkv");
            Assert.NotNull(item);

            Assert.Equal("100.Streets.2016.1080p.BluRay.6CH.x264.HeyDL.mkv", item.Name);
            Assert.Equal("http://dl8.heyserver.in/film/2018-4/100.Streets.2016.1080p.BluRay.6CH.x264.HeyDL.mkv", item.Url);
            Assert.Equal(1499060011, item.Size);
        }

        [Theory]
        [InlineData("aaa.bbb.ccc.1080p.xxx", "aaa.bbb.ccc")]
        [InlineData("Astral.2018.720p.WEB-DL.MkvCage.mkv", "Astral.2018")]
        public void ExtractNameTest(string name, string expected)
        {
            var outName = this.crawler.ExtractName(name);
            Assert.Equal(expected, outName);
        }

        [Fact]
        public async Task Should_Return_Biggest_Size()
        {
            var list = await this.crawler.GetLinks("http://dl8.heyserver.in/film/2018-11/");
            Assert.NotEmpty(list);
            var item = list.FirstOrDefault(p => p.Name.Contains("Searching.2018.1080p."));

            Assert.Equal("Searching.2018.1080p.BRRip.6CH.x264.MkvCage.mkv", item.Name);
            Assert.Equal(2068172847,item.Size);


            item = list.FirstOrDefault(p => p.Name.Contains("The.Nun.2018.1080p."));

            Assert.Equal("The.Nun.2018.1080p.BluRay.x264.Dubbed.mkv", item.Name);
            Assert.Equal(2028884531, item.Size);
        }

        [Fact]
        public async Task ExtractLinks_Should_Return_3Links()
        {
            var list = await this.crawler.ExtractLinks("https://gist.githubusercontent.com/samuraitruong/9fe07328a067a27bfd13d9ba01a5c80f/raw/e6403a8d7756c2ab7db2bb390d1401e627fe0930/legomovie4k.txt");
            Assert.NotEmpty(list);
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public async Task ExtractLinks_WithFilter_Should_Return_3Links()
        {
            var list = await this.crawler.ExtractLinks("https://gist.githubusercontent.com/samuraitruong/9fe07328a067a27bfd13d9ba01a5c80f/raw/e6403a8d7756c2ab7db2bb390d1401e627fe0930/legomovie4k.txt", "SBWAF2P6C1TQ", "WZ2W7YXJM6R4");
            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count);
        }

    }
}
