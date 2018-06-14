using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Benchmarks
{
    public class Drawing
    {
        public static int N { get; set; } = Parameters.N;
        public static string FileName { get; set; } = Parameters.FileName;
        public static int BufferSize { get; set; } = Parameters.BufferSize;

        public static byte[] DecompressImage()
        {
            byte[] data = new byte[BufferSize];

            for (int n = 0; n < N; n++)
            {
                using (var image = (Bitmap)Bitmap.FromFile(FileName))
                {
                    var imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

                    for (int i = 0; i < image.Height; i++)
                    {
                        IntPtr offset = imageData.Scan0 + i * imageData.Stride;
                        Marshal.Copy(offset, data, i * imageData.Stride, imageData.Stride);
                    }

                    image.UnlockBits(imageData);
                }
            }

            return data;
        }
    }
}
