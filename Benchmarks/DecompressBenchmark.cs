using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

namespace Benchmarks
{
    [CoreJob]
    public class DecompressBenchmark
    {
        [Benchmark]
        public void ImageSharpDecompress() => ImageSharp.DecompressImage();

        [Benchmark]
        public void LibPngDecompress() => LibPng.DecompressImage();

        [Benchmark]
        public void DrawingDecompress() => Drawing.DecompressImage();
    }
}
