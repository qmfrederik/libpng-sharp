using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Xunit;

namespace libpngsharp.Tests
{
    public class ImageSharpReferenceTest
    {
        [Fact]
        public void DecodeImageSharpTest()
        {
            byte[] data = null;

            using (Stream stream = File.OpenRead("screenshot.png"))
            {
                var image = Image.Load<Bgr24>(stream);

                data = new byte[image.PixelType.BitsPerPixel / 8 * image.Width * image.Height];
                image.Frames.RootFrame.SavePixelData(data);
            }

            SHA1 hasher = SHA1.Create();
            var hash = hasher.ComputeHash(data);
            var hashString = string.Join("", hash.Select(c => c.ToString("x2")));
            Assert.Equal(2181120, data.Length);
            Assert.Equal("43e046fbb27bfc352c6007247380966dc92f25e0", hashString);
        }
    }
}
