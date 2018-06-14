using System;
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
            var callback = new NativeMethods.png_rw(this.Read);
            NativeMethods.png_set_read_fn(this.pngPtr, IntPtr.Zero, callback);

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

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        public PngColorType ColorType
        {
            get;
            private set;
        }

        public int Channels
        {
            get;
            private set;
        }

        public int BitDepth
        {
            get;
            private set;
        }

        public string Version
        {
            get;
            private set;
        }

        public int BytesPerRow
        {
            get;
            private set;
        }

        public int BufferSize
        {
            get
            {
                return this.BytesPerRow * this.Height;
            }
        }

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

        public void Dispose()
        {
            NativeMethods.png_destroy_read_struct(ref this.pngPtr, ref this.infoPtr, ref this.endInfoPtr);
            NativeMethods.png_free(IntPtr.Zero, this.version);
        }

        private void Read(IntPtr png_ptr, void* outBytes, uint byteCountToRead)
        {
            Span<byte> target = new Span<byte>(outBytes, (int)byteCountToRead);
            this.Stream.Read(target);
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
