using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace libpngsharp
{
    public unsafe class PngDecoder : IDisposable
    {
        IntPtr version;
        IntPtr pngPtr;
        IntPtr infoPtr;
        IntPtr endInfoPtr = IntPtr.Zero;
        NativeMethods.png_rw readCallback;

        public PngDecoder(Stream stream)
        {
            this.Stream = stream ?? throw new ArgumentNullException(nameof(stream));

            this.version = NativeMethods.png_get_libpng_ver(IntPtr.Zero);
            this.Version = Marshal.PtrToStringAnsi(this.version);
            var error_fn = new NativeMethods.png_error(OnError);
            var warn_fn = new NativeMethods.png_error(OnWarning);

            this.pngPtr = NativeMethods.png_create_read_struct(this.version, new IntPtr(1), error_fn, warn_fn);
            ThrowOnZero(this.pngPtr);

            this.infoPtr = NativeMethods.png_create_info_struct(this.pngPtr);
            ThrowOnZero(this.infoPtr);

            // Set the callback function
            this.readCallback = new NativeMethods.png_rw(this.Read);
            NativeMethods.png_set_read_fn(this.pngPtr, IntPtr.Zero, this.readCallback);

            // Get basic image properties.
            // This will process all chunks up to but not including the image data.
            NativeMethods.png_read_info(this.pngPtr, this.infoPtr);

            this.Width = (int)NativeMethods.png_get_image_width(this.pngPtr, this.infoPtr);
            this.Height = (int)NativeMethods.png_get_image_height(this.pngPtr, this.infoPtr);
            this.BitDepth = NativeMethods.png_get_bit_depth(this.pngPtr, this.infoPtr);
            this.Channels = NativeMethods.png_get_channels(this.pngPtr, this.infoPtr);
            this.BytesPerRow = (int)NativeMethods.png_get_rowbytes(this.pngPtr, this.infoPtr);
            this.ColorType = NativeMethods.png_get_color_type(this.pngPtr, this.infoPtr);

            var number_of_passes = NativeMethods.png_set_interlace_handling(this.pngPtr);
            NativeMethods.png_read_update_info(this.pngPtr, this.infoPtr);
        }

        public Stream Stream
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the image width in pixels.
        /// </summary>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the image height in pixels.
        /// </summary>
        public int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the image color type.
        /// </summary>
        public PngColorType ColorType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of color channels in the image.
        /// </summary>
        public int Channels
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the image bit depth.
        /// </summary>
        public int BitDepth
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the version of libpng.
        /// </summary>
        public string Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of bytes in a row.
        /// </summary>
        public int BytesPerRow
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size of the decompressed image.
        /// </summary>
        public int DecompressedSize
        {
            get
            {
                return this.BytesPerRow * this.Height;
            }
        }

        /// <summary>
        /// Decodes the image into a decompressed buffer.
        /// </summary>
        /// <param name="buffer">
        /// A buffer into which to decompress the image. The length of this
        /// buffer must be at least <see cref="DecompressedSize"/>.
        /// </param>
        public void Decode(byte[] buffer)
        {
            // C# equivalent of:
            // http://zarb.org/~gc/html/libpng.html
            // http://pulsarengine.com/2009/01/reading-png-images-from-memory/

            fixed (byte* ptr = buffer)
            {
                IntPtr row_pointers = Marshal.AllocHGlobal((int)Marshal.SizeOf<IntPtr>() * (int)this.Height);

                IntPtr currentRow = new IntPtr(ptr);
                IntPtr row_pointer = row_pointers;

                for (int i = 0; i < this.Height; i++)
                {
                    var bytes = BitConverter.GetBytes(currentRow.ToInt64());
                    Marshal.Copy(bytes, 0, row_pointer, Marshal.SizeOf<IntPtr>());
                    currentRow += this.BytesPerRow;
                    row_pointer += Marshal.SizeOf<IntPtr>();
                }

                NativeMethods.png_read_image(this.pngPtr, row_pointers);
            }

            // Don't actually read the end_info data.
            NativeMethods.png_read_end(this.pngPtr, IntPtr.Zero);
        }

        /// <summary>
        /// Transforms paletted images to RGB.
        /// </summary>
        public void TransformPaletteToRgb()
        {
            NativeMethods.png_set_palette_to_rgb(this.pngPtr);
        }

        /// <summary>
        /// Transforms grayscale images of less than 8 to 8 bits
        /// </summary>
        public void TransformGrayTo8()
        {
            NativeMethods.png_set_gray_1_2_4_to_8(this.pngPtr);
        }

        /// <summary>
        /// For files with 16 bits per channel, strips the pixels down to 8 bit.
        /// </summary>
        public void TransformStrip16()
        {
            NativeMethods.png_set_strip_16(this.pngPtr);
        }

        /// <summary>
        /// Removes the alpha channel.
        /// </summary>
        public void TranformStripAlpha()
        {
            NativeMethods.png_set_strip_alpha(this.pngPtr);
        }

        /// <summary>
        /// Inverts the alpha channel, so that it represents transparancy instead of opacity.
        /// </summary>
        public void TransformInvertAlpha()
        {
            NativeMethods.png_set_invert_alpha(this.pngPtr);
        }

        /// <summary>
        /// Expands to 1 pixel per byte.
        /// </summary>
        public void TransformSetPacking()
        {
            NativeMethods.png_set_packing(this.pngPtr);
        }

        /// <summary>
        /// Changes the storage of pixels to blue, green, red.
        /// </summary>
        public void TransformSetBgr()
        {
            NativeMethods.png_set_bgr(this.pngPtr);
        }

        /// <summary>
        /// Transforms the data to ARGB instead of the normal PNG format RGBA.
        /// </summary>
        public void TransformSwapAlpha()
        {
            NativeMethods.png_set_swap_alpha(this.pngPtr);
        }

        /// <summary>
        /// Represents a grayscal image as a RGB image.
        /// </summary>
        public void TransformGrayToRgb()
        {
            NativeMethods.png_set_gray_to_rgb(this.pngPtr);
        }

        /// <summary>
        /// Inverts the black and white pixels in a monochrome image.
        /// </summary>
        public void TransformInvertMono()
        {
            NativeMethods.png_set_invert_mono(this.pngPtr);
        }

        /// <summary>
        /// Changes the pixel byte order for 16-bit pixels from bit-endian to little-endian.
        /// </summary>
        public void TransformSwap()
        {
            NativeMethods.png_set_swap(this.pngPtr);
        }

        /// <summary>
        /// Swaps the onder in which pixels are packed into bytes.
        /// </summary>
        public void TransformPackswap()
        {
            NativeMethods.png_set_packswap(this.pngPtr);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            NativeMethods.png_destroy_read_struct(ref this.pngPtr, ref this.infoPtr, ref this.endInfoPtr);
            NativeMethods.png_free(IntPtr.Zero, this.version);
        }

        private void Read(IntPtr png_ptr, void* outBytes, uint byteCountToRead)
        {
#if NETSTANDARD2_0 || NETCOREAPP2_0
            byte[] buffer = null;

            try
            {
                buffer = ArrayPool<byte>.Shared.Rent((int)byteCountToRead);
                this.Stream.Read(buffer, 0, (int)byteCountToRead);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
#else
            Span<byte> target = new Span<byte>(outBytes, (int)byteCountToRead);
            this.Stream.Read(target);
#endif
        }

        private void OnError(IntPtr png_structp, IntPtr png_const_charp)
        {
            var error = Marshal.PtrToStringAnsi(png_const_charp);
        }

        private void OnWarning(IntPtr png_structp, IntPtr png_const_charp)
        {
            var error = Marshal.PtrToStringAnsi(png_const_charp);
        }

        private void ThrowOnZero(IntPtr value)
        {
            if (value == IntPtr.Zero)
            {
                throw new Exception();
            }
        }
    }
}
