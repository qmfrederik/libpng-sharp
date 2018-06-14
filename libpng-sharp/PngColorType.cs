using System;

namespace libpngsharp
{
    [Flags]
    public enum PngColorType : byte
    {
        /* These describe the color_type field in png_info. */
        /* color type masks */
        PNG_COLOR_MASK_PALETTE = 1,
        PNG_COLOR_MASK_COLOR = 2,
        PNG_COLOR_MASK_ALPHA = 4,

        /* color types.  Note that not all combinations are legal */
        PNG_COLOR_TYPE_GRAY = 0,
        PNG_COLOR_TYPE_PALETTE = (PNG_COLOR_MASK_COLOR | PNG_COLOR_MASK_PALETTE),
        PNG_COLOR_TYPE_RGB = (PNG_COLOR_MASK_COLOR),
        PNG_COLOR_TYPE_RGB_ALPHA = (PNG_COLOR_MASK_COLOR | PNG_COLOR_MASK_ALPHA),
        PNG_COLOR_TYPE_GRAY_ALPHA = (PNG_COLOR_MASK_ALPHA),

        /* aliases */
        PNG_COLOR_TYPE_RGBA = PNG_COLOR_TYPE_RGB_ALPHA,
        PNG_COLOR_TYPE_GA = PNG_COLOR_TYPE_GRAY_ALPHA
    }
}
