using BenchmarkDotNet.Running;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            ImageSharp.DecompressImage();
            // LibPng.DecompressImage();
            Drawing.DecompressImage();
#else
            var summary = BenchmarkRunner.Run<DecompressBenchmark>();
#endif
        }
    }
}
