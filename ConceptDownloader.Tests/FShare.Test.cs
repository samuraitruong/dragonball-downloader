using System;
using System.Threading.Tasks;
using ConceptDownloader.Services;
using Xunit;

namespace ConceptDownloader.Tests
{
    public class FShareTest
    {
        public FShareTest()
        {
        }
        [Fact]
        public async Task GetLinks_Should_Success()
        {
            var fs = new Fshare();
            var result = await fs.GetLink("https://www.fshare.vn/file/3GSD5D48JQUS");
            Assert.NotNull(result);
            Assert.Equal("Nan.bei.Shao.Lin.1986.1080p.BluRay.x264-WiKi.srt", result.Name );
        }

        [Fact]
        public async Task GetFileName_Should_Success()
        {
            var fs = new Fshare();
            var result = await fs.GetFileName("https://www.fshare.vn/file/3GSD5D48JQUS");
            Assert.NotNull(result);
            Assert.Equal("Nan.bei.Shao.Lin.1986.1080p.BluRay.x264-WiKi.srt", result);
        }

        //https://www.fshare.vn/folder/WWKS2LJNCI8E

        [Fact]
        public async Task GetFileInFolder_Should_Success()
        {
            var fs = new Fshare();
            var result = await fs.GetFilesInFolder("https://www.fshare.vn/folder/WWKS2LJNCI8E?token=1545855952");
            Assert.NotNull(result);
            Assert.NotEmpty(result.Items);
            Assert.Equal(131, result.Items.Count);
            //Assert.Equal("Nan.bei.Shao.Lin.1986.1080p.BluRay.x264-WiKi.srt", result);
        }

    }
}
