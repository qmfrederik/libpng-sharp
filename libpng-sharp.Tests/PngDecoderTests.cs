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
                Assert.Equal(0x00214800, decoder.BufferSize);
                Assert.Equal(0x780, decoder.BytesPerRow);
                Assert.Equal(3, decoder.Channels);
                Assert.Equal(PngColorType.RGB, decoder.ColorType);
                Assert.Equal(1136, decoder.Height);
                Assert.Equal(stream, decoder.Stream);
                Assert.Equal("1.6.34", decoder.Version);
                Assert.Equal(640, decoder.Width);

                byte[] data = new byte[decoder.BufferSize];
                decoder.Decode(data);

                SHA1 hasher = SHA1.Create();
                var hash = hasher.ComputeHash(data);
                var hashString = string.Join("", hash.Select(c => c.ToString("x2")));
                Assert.Equal("93f4adc02e4c8e1215a523c7a8f0641afe2b5eb3", hashString);

                File.WriteAllBytes("png.raw", data);
            }
        }
    }
}
