using System;
using System.IO;
using System.Runtime.InteropServices;

namespace libpngsharp
{
    public class PngDecoder
    {
        public Stream Stream
        {
            get;
            set;
        }

        public unsafe void Decode(byte[] buffer)
        {
            // C# equivalent of:
            // http://zarb.org/~gc/html/libpng.html
            // http://pulsarengine.com/2009/01/reading-png-images-from-memory/
            var ver = NativeMethods.png_get_libpng_ver(IntPtr.Zero);
            var error_fn = new NativeMethods.png_error(OnError);
            var warn_fn = new NativeMethods.png_error(OnWarning);

            var png_ptr = NativeMethods.png_create_read_struct(ver, new IntPtr(1), error_fn, warn_fn);
            ThrowOnZero(png_ptr);

            var info_ptr = NativeMethods.png_create_info_struct(png_ptr);
            ThrowOnZero(info_ptr);

            // Set the callback function
            var callback = new NativeMethods.png_rw(Read);
            NativeMethods.png_set_read_fn(png_ptr, IntPtr.Zero, callback);

            NativeMethods.png_read_info(png_ptr, info_ptr);

            var width = NativeMethods.png_get_image_width(png_ptr, info_ptr);
            var height = NativeMethods.png_get_image_height(png_ptr, info_ptr);
            var color_type = (PngColorType)NativeMethods.png_get_color_type(png_ptr, info_ptr);
            var channels = NativeMethods.png_get_channels(png_ptr, info_ptr);
            var bit_depth = NativeMethods.png_get_bit_depth(png_ptr, info_ptr);

            var number_of_passes = NativeMethods.png_set_interlace_handling(png_ptr);
            NativeMethods.png_read_update_info(png_ptr, info_ptr);

            var bytesPerRow = NativeMethods.png_get_rowbytes(png_ptr, info_ptr);
            var cb = (int)(bytesPerRow * height);

            fixed (byte* ptr = buffer)
            {
                IntPtr row_pointers = Marshal.AllocHGlobal((int)Marshal.SizeOf<IntPtr>() * (int)height);

                IntPtr currentRow = new IntPtr(ptr);
                IntPtr row_pointer = row_pointers;

                for (int i = 0; i < height; i++)
                {
                    var bytes = BitConverter.GetBytes(currentRow.ToInt64());
                    Marshal.Copy(bytes, 0, row_pointer, Marshal.SizeOf<IntPtr>());
                    currentRow += (int)bytesPerRow;
                    row_pointer += Marshal.SizeOf<IntPtr>();
                }

                NativeMethods.png_read_image(png_ptr, row_pointers);
            }

            GC.KeepAlive(callback);
        }

        private unsafe void Read(IntPtr png_ptr, void* outBytes, uint byteCountToRead)
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
