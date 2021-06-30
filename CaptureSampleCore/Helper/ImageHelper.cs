using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CaptureCore
{
    public class ImageHelper
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static int GetBmpMemorySize(int width, int height, bool v5header = true)
        {
            int size = width * height * 4;
            int dibHeaderSize = v5header ? 124 : 40; // BITMAPINFOHEADER: 40, BITMAPV5HEADER: 136
            const int extraSize = 12;
            int bmpSize = size + 14 + dibHeaderSize + extraSize /* BMP header size */;
            return bmpSize;
        }

        public static IntPtr CreateBmp(IntPtr imageData, int width, int height, out int resultSize)
        {
            int size = width * height * 4;
            const bool v5header = true;
            const int dibHeaderSize = v5header ? 124 : 40; // BITMAPINFOHEADER: 40, BITMAPV5HEADER: 136
            const int extraSize = 12;
            int bmpSize = size + 14 + dibHeaderSize + extraSize /* BMP header size */;
            var pnt = Marshal.AllocHGlobal(bmpSize);

            unsafe
            {
                var p = (byte*)pnt;
                *p++ = (byte)'B';
                *p++ = (byte)'M';

                int num = bmpSize;
                *p++ = (byte)(num & 0xff);
                num >>= 8;
                *p++ = (byte)(num & 0xff);
                num >>= 8;
                *p++ = (byte)(num & 0xff);
                num >>= 8;
                *p++ = (byte)(num & 0xff);

                // Reserved
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;

                // The offset, i.e. starting address, of the byte where the bitmap image data (pixel array) can be found.
                *p++ = dibHeaderSize + extraSize + 14;
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;

                // DIB header:

                // the size of this header, in bytes
                *p++ = dibHeaderSize;
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;

                // the bitmap width in pixels (signed integer)
                num = width;
                *p++ = (byte)(num & 0xff);
                num >>= 8;
                *p++ = (byte)(num & 0xff);
                num >>= 8;
                *p++ = (byte)(num & 0xff);
                num >>= 8;
                *p++ = (byte)(num & 0xff);

                // the bitmap height in pixels (signed integer)
                num = height;
                *p++ = (byte)(num & 0xff);
                num >>= 8;
                *p++ = (byte)(num & 0xff);
                num >>= 8;
                *p++ = (byte)(num & 0xff);
                num >>= 8;
                *p++ = (byte)(num & 0xff);

                // the number of color planes (must be 1)
                *p++ = 1;
                *p++ = 0;

                // the number of bits per pixel, which is the color depth of the image. Typical values are 1, 4, 8, 16, 24 and 32.
                *p++ = 32;
                *p++ = 0;

                // the compression method being used. See the next table for a list of possible values
                *p++ = 3;
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;

                // the image size. This is the size of the raw bitmap data; a dummy 0 can be given for BI_RGB bitmaps.
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;

                // the horizontal resolution of the image. (pixel per metre, signed integer) : 96 dpi
                *p++ = 196;
                *p++ = 14;
                *p++ = 0;
                *p++ = 0;

                // the vertical resolution of the image. (pixel per metre, signed integer)
                *p++ = 196;
                *p++ = 14;
                *p++ = 0;
                *p++ = 0;

                // the number of colors in the color palette, or 0 to default to 2n
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;

                // the number of important colors used, or 0 when every color is important; generally ignored
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;
                *p++ = 0;

                if (v5header)
                {
                    // red mask
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0xff;
                    *p++ = 0;

                    // green mask
                    *p++ = 0;
                    *p++ = 0xff;
                    *p++ = 0;
                    *p++ = 0;

                    // blue mask
                    *p++ = 0xff;
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;

                    // alpha mask
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0xff;

                    // The color space of the DIB.
                    *p++ = 0x20;
                    *p++ = 0x6e;
                    *p++ = 0x69;
                    *p++ = 0x57;

                    // A CIEXYZTRIPLE structure that specifies the x, y, and z coordinates of the three colors that correspond to
                    // the red, green, and blue endpoints for the logical color space associated with the bitmap.
                    // This member is ignored unless the bV5CSType member specifies LCS_CALIBRATED_RGB.
                    for (int i = 0; i < 36; i++)
                    {
                        *p++ = 0;
                    }

                    // Toned response curve for red.
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;

                    // Toned response curve for green.
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;

                    // Toned response curve for blue.
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;

                    // Rendering intent for bitmap.
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;

                    // The offset, in bytes, from the beginning of the BITMAPV5HEADER structure to the start of the profile data.
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;

                    // Size, in bytes, of embedded profile data.
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;

                    // Reserved
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;
                    *p++ = 0;

                    p += extraSize; // ?
                }

                int stride = width * 4;
                for (int i = 0; i < height; i++)
                {
                    Buffer.MemoryCopy((void*)(imageData + (height - i - 1) * stride), p, stride, stride);
                    p += stride;
                }
            }

            resultSize = bmpSize;
            return pnt;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

    }
}