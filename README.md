# libpng-sharp

This repository contains some sample code on how you can call libpng from C#.

The code was written to test whether calling libpng natively could be an alternative, performance-wise,
for System.Drawing to decode PNG images.

What I found out was:
- Decoding PNG images very slow compared to other formats (for example, jpeg using libjpeg-turbo)
- GDI+ seemed to be at least as performant as libpng on Windows
- On Mono, System.Drawing uses libgdiplus which uses libpng under the hood.

Net, I'm not continuing the project at this time.
