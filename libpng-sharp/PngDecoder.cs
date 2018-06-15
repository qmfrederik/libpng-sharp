using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace libpngsharp
{
    /// <summary>
    /// Decodes PNG images using the libpng library.
    /// </summary>
    public unsafe class PngDecoder : IDisposable
    {
        IntPtr version;
        IntPtr pngPtr;
        IntPtr infoPtr;
        IntPtr endInfoPtr = IntPtr.Zero;
        NativeMethods.png_rw readCallback;
        NativeMethods.png_error errorCallback;
        NativeMethods.png_error warningCallback;
        bool transformationsSaved = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PngDecoder"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which contains a PNG image.
        /// </param>
        public PngDecoder(Stream stream)
        {
            this.Stream = stream ?? throw new ArgumentNullException(nameof(stream));

            this.version = NativeMethods.png_get_libpng_ver(IntPtr.Zero);
            this.Version = Marshal.PtrToStringAnsi(this.version);
            this.errorCallback = new NativeMethods.png_error(OnError);
            this.warningCallback = new NativeMethods.png_error(OnWarning);

            this.pngPtr = NativeMethods.png_create_read_struct(this.version, new IntPtr(1), this.errorCallback, this.warningCallback);
            ThrowOnZero(this.pngPtr);

            this.infoPtr = NativeMethods.png_create_info_struct(this.pngPtr);
            ThrowOnZero(this.infoPtr);

            // Set the callback function
            this.readCallback = new NativeMethods.png_rw(this.Read);
            NativeMethods.png_set_read_fn(this.pngPtr, IntPtr.Zero, this.readCallback);

            // Get basic image properties.
            // This will process all chunks up to but not including the image data.
            NativeMethods.png_read_info(this.pngPtr, this.infoPtr);

            this.RefreshProperties();
        }

        /// <summary>
        /// The event which is raised when an error occurs.
        /// </summary>
        public event EventHandler<string> Error;

        /// <summary>
        /// The event which is raised when a warning occurs.
        /// </summary>
        public event EventHandler<string> Warning;

        /// <summary>
        /// The <see cref="Stream"/> from which to read the PNG image.
        /// </summary>
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
            if (!this.transformationsSaved)
            {
                this.SaveTransformations();
            }

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
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_palette_to_rgb(this.pngPtr);
        }

        /// <summary>
        /// Transforms grayscale images of less than 8 to 8 bits
        /// </summary>
        public void TransformGrayTo8()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_gray_1_2_4_to_8(this.pngPtr);
        }

        /// <summary>
        /// For files with 16 bits per channel, strips the pixels down to 8 bit.
        /// </summary>
        public void TransformStrip16()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_strip_16(this.pngPtr);
        }

        /// <summary>
        /// Removes the alpha channel.
        /// </summary>
        public void TranformStripAlpha()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_strip_alpha(this.pngPtr);
        }

        /// <summary>
        /// Inverts the alpha channel, so that it represents transparancy instead of opacity.
        /// </summary>
        public void TransformInvertAlpha()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_invert_alpha(this.pngPtr);
        }

        /// <summary>
        /// Adds an alpha channel to an 8-bit grayscal iimage or 24-bit RGB image.
        /// </summary>
        public void TransformAddAlpha()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_add_alpha(this.pngPtr);
        }

        /// <summary>
        /// Adds a filler byte when an 8-bit grayscale image or 24-bit RGB image is read.
        /// </summary>
        /// <param name="filler">
        /// The filler byte to add.
        /// </param>
        /// <param name="flags">
        /// Flags controlling how to add the filler.
        /// </param>
        public void TransformSetFiller(uint filler, PngFillerFlags flags)
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_filler(this.pngPtr, filler, flags);
        }

        /// <summary>
        /// Expands to 1 pixel per byte.
        /// </summary>
        public void TransformSetPacking()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_packing(this.pngPtr);
        }

        /// <summary>
        /// Changes the storage of pixels to blue, green, red.
        /// </summary>
        public void TransformSetBgr()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_bgr(this.pngPtr);
        }

        /// <summary>
        /// Transforms the data to ARGB instead of the normal PNG format RGBA.
        /// </summary>
        public void TransformSwapAlpha()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_swap_alpha(this.pngPtr);
        }

        /// <summary>
        /// Represents a grayscal image as a RGB image.
        /// </summary>
        public void TransformGrayToRgb()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_gray_to_rgb(this.pngPtr);
        }

        /// <summary>
        /// Inverts the black and white pixels in a monochrome image.
        /// </summary>
        public void TransformInvertMono()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_invert_mono(this.pngPtr);
        }

        /// <summary>
        /// Changes the pixel byte order for 16-bit pixels from bit-endian to little-endian.
        /// </summary>
        public void TransformSwap()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_swap(this.pngPtr);
        }

        /// <summary>
        /// Swaps the onder in which pixels are packed into bytes.
        /// </summary>
        public void TransformPackswap()
        {
            if (this.transformationsSaved)
            {
                throw new InvalidOperationException();
            }

            NativeMethods.png_set_packswap(this.pngPtr);
        }

        /// <summary>
        /// Saves the transformations, and updates the properties (such as <see cref="BitDepth"/>) to match the
        /// transformed image.
        /// </summary>
        /// <remarks>
        /// You can only call <see cref="SaveTransformations"/> once. You can not call any of the <c>Transform*</c>
        /// methods after you have saved the transformations.
        /// </remarks>
        public void SaveTransformations()
        {
            // C# equivalent of:
            // http://zarb.org/~gc/html/libpng.html
            // http://pulsarengine.com/2009/01/reading-png-images-from-memory/
            NativeMethods.png_read_update_info(this.pngPtr, this.infoPtr);
            this.transformationsSaved = true;
            this.RefreshProperties();
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

        private void RefreshProperties()
        {
            this.Width = (int)NativeMethods.png_get_image_width(this.pngPtr, this.infoPtr);
            this.Height = (int)NativeMethods.png_get_image_height(this.pngPtr, this.infoPtr);
            this.BitDepth = NativeMethods.png_get_bit_depth(this.pngPtr, this.infoPtr);
            this.Channels = NativeMethods.png_get_channels(this.pngPtr, this.infoPtr);
            this.BytesPerRow = (int)NativeMethods.png_get_rowbytes(this.pngPtr, this.infoPtr);
            this.ColorType = NativeMethods.png_get_color_type(this.pngPtr, this.infoPtr);
        }

        private void OnError(IntPtr png_structp, IntPtr png_const_charp)
        {
            var error = Marshal.PtrToStringAnsi(png_const_charp);
            this.Error?.Invoke(this, error);
        }

        private void OnWarning(IntPtr png_structp, IntPtr png_const_charp)
        {
            var error = Marshal.PtrToStringAnsi(png_const_charp);
            this.Warning?.Invoke(this, error);
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
