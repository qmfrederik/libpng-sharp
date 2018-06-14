using libpngsharp;
using System.IO;

namespace Benchmarks
{
    public class LibPng
    {
        public static int N { get; set; } = Parameters.N;
        public static string FileName { get; set; } = Parameters.FileName;
        public static int BufferSize { get; set; } = Parameters.BufferSize;

        public static byte[] DecompressImage()
        {
            byte[] buffer = new byte[BufferSize];

            using (Stream stream = File.OpenRead(FileName))
            {
                for (int i = 0; i < N; i++)
                {
                    stream.Position = 0;
                    PngDecoder png = new PngDecoder(stream);
                    png.TransformSetBgr();
                    png.Decode(buffer);
                }
            }

            return buffer;
        }
    }
}
