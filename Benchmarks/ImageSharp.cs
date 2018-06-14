using SixLabors.ImageSharp;
using System.IO;

namespace Benchmarks
{
    public class ImageSharp
    {
        public static int N { get; set; } = Parameters.N;
        public static string FileName { get; set; } = Parameters.FileName;
        public static int BufferSize { get; set; } = Parameters.BufferSize;

        public static byte[] DecompressImage()
        {
            byte[] buffer = new byte[BufferSize];
            byte[] data = File.ReadAllBytes(FileName);

            for (int n = 0; n < N; n++)
            {
                var image = Image.Load(data);
                image.Frames.RootFrame.SavePixelData(buffer);
            }

            return buffer;
        }
    }
}
