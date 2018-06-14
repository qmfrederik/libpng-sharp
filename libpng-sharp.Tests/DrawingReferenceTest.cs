using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Xunit;

namespace libpngsharp.Tests
{
    public class DrawingReferenceTest
    {
        [Fact]
        public void DecodeDrawingTest()
        {
            byte[] data = null;

            using (var image = (Bitmap)Bitmap.FromFile("screenshot.png"))
            {
                var imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

                data = new byte[imageData.Height * imageData.Stride];

                for (int i = 0; i < image.Height; i++)
                {
                    IntPtr offset = imageData.Scan0 + i * imageData.Stride;
                    Marshal.Copy(offset, data, i * imageData.Stride, imageData.Stride);
                }

                image.UnlockBits(imageData);
            }

            File.WriteAllBytes("drawing.raw", data);

            SHA1 hasher = SHA1.Create();
            var hash = hasher.ComputeHash(data);
            var hashString = string.Join("", hash.Select(c => c.ToString("x2")));

            // Different pixel format
            Assert.Equal("43e046fbb27bfc352c6007247380966dc92f25e0", hashString);
        }
    }
}
