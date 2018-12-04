using System;
using System.Threading.Tasks;
using ConceptDownloader;
using Xunit;

namespace ConceptDownloader.Tests
{
    public class SimpleCrawlerTest
    {
        [Fact]
        public async Task Test1()
        {
            var list = await SimpleCrawler.GetLinks("http://dl8.heyserver.in/film/2018-11/");
            Assert.NotEmpty(list);

            Assert.Equal("1987.When.the.Day.Comes.2017.1080p.BluRay.x264.6CH-Pahe.mkv", list[0].Name);
            Assert.Equal("http://dl8.heyserver.in/film/2018-11/1987.When.the.Day.Comes.2017.1080p.BluRay.x264.6CH-Pahe.mkv", list[0].Url);
        }
    }
}
