using System;
using System.Threading.Tasks;
using ConceptDownloader.Services;
using Xunit;

namespace ConceptDownloader.Tests
{
    public class FShareTest
    {
        private string username = "samuraitruong@hotmail.com";
        private string password = "@bhu8*UHB!";
        public FShareTest()
        {
        }
        [Fact]
        public async Task GetLinks_Should_Success()
        {
            var fs = new Fshare(username, password);
            var result = await fs.GetLink("https://www.fshare.vn/file/3GSD5D48JQUS");
            Assert.NotNull(result);
            Assert.Equal("Nan.bei.Shao.Lin.1986.1080p.BluRay.x264-WiKi.srt", result.Name );
        }

        [Fact]
        public async Task GetFileName_Should_Success()
        {
            var fs = new Fshare(username, password);
            var result = await fs.GetFileName("https://www.fshare.vn/file/3GSD5D48JQUS");
            Assert.NotNull(result);
            Assert.Equal("Nan.bei.Shao.Lin.1986.1080p.BluRay.x264-WiKi.srt", result);
        }

        //https://www.fshare.vn/folder/WWKS2LJNCI8E

        [Fact]
        public async Task GetFileInFolder_Should_Success()
        {
            var fs = new Fshare(username, password);
            var result = await fs.GetFilesInFolder("https://www.fshare.vn/folder/WWKS2LJNCI8E?token=1545855952");
            Assert.NotNull(result);
            Assert.NotEmpty(result.Items);
            Assert.Equal(131, result.Items.Count);
            //Assert.Equal("Nan.bei.Shao.Lin.1986.1080p.BluRay.x264-WiKi.srt", result);
        }
        [Fact]
        public async Task Login_Should_Success()
        {
            var fs = new Fshare(username, password);

            var result = await fs.Login();
            Assert.True(result);
        }
        [Fact]
        public async Task GetLink_Should_Success()
        {
            var fs = new Fshare(username, password);

            var result = await fs.GetLink("https://www.fshare.vn/file/6Y4FHUK3C8S3");
            Assert.NotNull(result);
            Assert.Contains("Skyscraper.2018.1080p.BluRay.DD5.1.x264-TayTO Vietnamese.srt", result.Url);
        }


    }
}
