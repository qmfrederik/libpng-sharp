# libpng-sharp

This repository contains some sample code on how you can call libpng from C#.

The code was written to test whether calling libpng natively could be an alternative, performance-wise,
for System.Drawing to decode PNG images.

What I found out was:
- Decoding PNG images very slow compared to other formats (for example, jpeg using libjpeg-turbo)
- GDI+ seemed to be at least as performant as libpng on Windows
- On Mono, System.Drawing uses libgdiplus which uses libpng under the hood.

Net, I'm not continuing the project at this time.


## Benchmark Results

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

### macOS
