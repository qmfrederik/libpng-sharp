# libpng-sharp
[![Build status](https://ci.appveyor.com/api/projects/status/dhg5k9wvefs6q33d?svg=true)](https://ci.appveyor.com/project/qmfrederik/libpng-sharp) [![Build Status](https://travis-ci.org/qmfrederik/libpng-sharp.svg?branch=master)](https://travis-ci.org/qmfrederik/libpng-sharp)

This repository contains some sample code on how you can call libpng from C#.

The code was written to test whether calling libpng natively could be an alternative, performance-wise,
for System.Drawing to decode PNG images.

What I found out was:
- Decoding PNG images very slow compared to other formats (for example, jpeg using libjpeg-turbo)
- GDI+ seemed to be at least as performant as libpng on Windows, but libpng is a winner on Linux and macOS
- On Mono, System.Drawing uses libgdiplus which uses libpng under the hood.

Conclusion:
- On Windows, you may want to keep using System.Drawing
- On Linux and macOS, libpng is a better option, performance-wise

libpng uses zlib, of which various forks exist which contain additional performance improvements. These forks
have not been tested.

## Benchmark Results - decoding to library-native format

There are Travis and AppVeyor builds which run benchmarks. The list below gives a snapshot of the results:

### Windows

```
// * Summary *

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Xeon CPU E5-2697 v3 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=2.1.300
  [Host] : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT
  Core   : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT

Job=Core  Runtime=Core

               Method |       Mean |     Error |    StdDev |
--------------------- |-----------:|----------:|----------:|
 ImageSharpDecompress | 1,132.6 ms | 13.356 ms | 12.493 ms |
     LibPngDecompress |   932.8 ms |  4.741 ms |  4.435 ms |
    DrawingDecompress |   783.0 ms |  5.968 ms |  5.290 ms |
```

### Linux

```
// * Summary *

BenchmarkDotNet=v0.10.14, OS=ubuntu 16.04
Intel Xeon CPU 2.50GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=2.1.300
  [Host] : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT
  Core   : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT

Job=Core  Runtime=Core  

               Method |    Mean |    Error |   StdDev |
--------------------- |--------:|---------:|---------:|
 ImageSharpDecompress | 1.408 s | 0.0273 s | 0.0335 s |
     LibPngDecompress | 1.227 s | 0.0208 s | 0.0195 s |
    DrawingDecompress |      NA |       NA |       NA |
```

### macOS

```
// * Summary *

BenchmarkDotNet=v0.10.14, OS=macOS Sierra 10.12.6 (16G29) [Darwin 16.7.0]
Intel Xeon CPU E5-2697 v2 2.70GHz, 2 CPU, 2 logical and 2 physical cores
.NET Core SDK=2.1.300
  [Host] : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT
  Core   : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT

Job=Core  Runtime=Core

               Method |    Mean |    Error |   StdDev |
--------------------- |--------:|---------:|---------:|
 ImageSharpDecompress | 1.266 s | 0.0245 s | 0.0229 s |
     LibPngDecompress | 1.027 s | 0.0156 s | 0.0146 s |
    DrawingDecompress | 1.481 s | 0.0042 s | 0.0033 s |
```

## Benchmark Results - Decoding to BGR24

### Windows

```
// * Summary *

NET Core SDK=2.1.300
  [Host] : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT
  Core   : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT

Job=Core  Runtime=Core

               Method |       Mean |     Error |    StdDev |
--------------------- |-----------:|----------:|----------:|
 ImageSharpDecompress | 1,607.5 ms |  4.248 ms |  3.973 ms |
     LibPngDecompress | 1,153.8 ms | 10.889 ms | 10.186 ms |
    DrawingDecompress |   948.6 ms | 18.685 ms | 19.992 ms |
```

### Linux

```
// * Summary *

BenchmarkDotNet=v0.10.14, OS=ubuntu 16.04
Intel Xeon CPU 2.50GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=2.1.300
  [Host] : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT
  Core   : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT

Job=Core  Runtime=Core  9m

               Method |    Mean |    Error |   StdDev |
--------------------- |--------:|---------:|---------:|
 ImageSharpDecompress | 1.672 s | 0.0334 s | 0.0397 s |
     LibPngDecompress | 1.226 s | 0.0209 s | 0.0195 s |
    DrawingDecompress | 1.896 s | 0.0250 s | 0.0234 s |
```

### macOS

```
// * Summary *
BenchmarkDotNet=v0.10.14, OS=macOS Sierra 10.12.6 (16G29) [Darwin 16.7.0]
Intel Xeon CPU E5-2697 v2 2.70GHz, 2 CPU, 2 logical and 2 physical cores
.NET Core SDK=2.1.300
  [Host] : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT
  Core   : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT

Job=Core  Runtime=Core

               Method |    Mean |    Error |   StdDev |9m
--------------------- |--------:|---------:|---------:|
 ImageSharpDecompress | 1.537 s | 0.0300 s | 0.0281 s |
     LibPngDecompress | 1.073 s | 0.0145 s | 0.0135 s |
    DrawingDecompress | 1.497 s | 0.0116 s | 0.0108 s |
```
