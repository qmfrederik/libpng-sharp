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
            PngDecoder png = new PngDecoder();
            png.Stream = File.OpenRead(FileName);
            byte[] buffer = new byte[BufferSize];

            for (int i = 0; i < N; i++)
            {
                png.Stream.Position = 0;
                png.Decode(buffer);
            }

            return buffer;
        }
    }
}
