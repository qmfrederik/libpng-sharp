using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Xunit;

namespace libpngsharp.Tests
{
    public class PngDecoderTests
    {
        [Fact]
        public void DecodePngImageTest()
        {
            using (Stream stream = File.OpenRead("screenshot.png"))
            using (PngDecoder decoder = new PngDecoder(stream))
            {
                Assert.Equal(8, decoder.BitDepth);
                Assert.Equal(0x00214800, decoder.DecompressedSize);
                Assert.Equal(0x780, decoder.BytesPerRow);
                Assert.Equal(3, decoder.Channels);
                Assert.Equal(PngColorType.RGB, decoder.ColorType);
                Assert.Equal(1136, decoder.Height);
                Assert.Equal(stream, decoder.Stream);

                // The revision can differ, e.g. 1.16.34 on Windows and 1.16.20 on Ubuntu Xenial, so don't check the
                // entire string.
                Assert.NotNull(decoder.Version);
                var version = new Version(decoder.Version);
                Assert.Equal(1, version.Major);

                Assert.Equal(640, decoder.Width);

                byte[] data = new byte[decoder.DecompressedSize];
                decoder.TransformSetBgr();
                decoder.Decode(data);

                SHA1 hasher = SHA1.Create();
                var hash = hasher.ComputeHash(data);
                var hashString = string.Join("", hash.Select(c => c.ToString("x2")));
                Assert.Equal(2181120, data.Length);
                Assert.Equal("43e046fbb27bfc352c6007247380966dc92f25e0", hashString);
            }
        }
    }
}
