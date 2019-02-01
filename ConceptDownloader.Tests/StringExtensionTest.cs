using System;
using ConceptDownloader.Extensions;
using Xunit;

namespace ConceptDownloader.Tests
{
    public class StringExtensionTest
    {
        public StringExtensionTest()
        {
        }
        [Fact]
        public void Encrypt_With_EAS_Should_SUccess()
        {
            ClsCrypto crpto = new ClsCrypto("1228418106");

            string output = crpto.Encrypt("https://www.fshare.vn/file/WGBWBLPLIHNK");
            Assert.Equal("U2FsdGVkX19s8No7nokqpSmqlENVnyZSv8tjMSlB9xuVvzCzOybvROTmcge8uzZX2t6u4V/gQ8NekqWdTVumqw==", output);
        }
    }
}
