using System;
using System.Threading.Tasks;
using ConceptDownloader.Services;
using Xunit;

namespace ConceptDownloader.Tests
{
    public class AnLinkTopTests
    {
        public AnLinkTopTests()
        {
        }
        [Fact]
        public async Task GetLink_Should_Success()
        {
            var service = new AnLinkTop();
            var result = await service.GetLink("https://www.fshare.vn/file/X4EEJAG6AJOGY93");

            Assert.NotNull(result);
            //Assert.Contains("dl?id=", result.Url);
            Assert.Equal("Kong.Skull.Island.2017.ViE.1080p.BluRay.DTS.x264-SPARKS.mkv", result.Name);
        }
    }
}
