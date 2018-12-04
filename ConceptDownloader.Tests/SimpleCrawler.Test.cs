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
        }
    }
}
