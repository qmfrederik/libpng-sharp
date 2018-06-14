using System;
using System.Runtime.InteropServices;

namespace libpngsharp
{
    internal class NativeMethods
    {
        private const string Library = "png16";

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr png_get_libpng_ver(IntPtr png_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr png_create_read_struct(IntPtr user_png_ver, IntPtr error_ptr, png_error error_fn, png_error warn_fn);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr png_create_info_struct(IntPtr png_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void png_set_read_fn(IntPtr png_ptr, IntPtr io_ptr, [MarshalAs(UnmanagedType.FunctionPtr)]png_rw read_data_fn);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void png_read_info(IntPtr png_ptr, IntPtr info_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint png_get_image_width(IntPtr png_ptr, IntPtr info_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint png_get_image_height(IntPtr png_ptr, IntPtr info_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte png_get_color_type(IntPtr png_ptr, IntPtr info_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte png_get_channels(IntPtr png_ptr, IntPtr info_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte png_get_bit_depth(IntPtr png_ptr, IntPtr info_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern int png_set_interlace_handling(IntPtr png_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void png_read_update_info(IntPtr png_ptr, IntPtr info_ptr);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void png_read_image(IntPtr png_ptr, IntPtr image);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint png_get_rowbytes(IntPtr png_ptr, IntPtr info_ptr);

        public unsafe delegate void png_rw(IntPtr png_ptr, void* outBytes, uint byteCountToRead);

        public delegate void png_error(IntPtr png_structp, IntPtr png_const_charp);
    }
}
