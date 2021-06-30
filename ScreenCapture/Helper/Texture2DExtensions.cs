using System;
using System.Drawing.Imaging;
using CaptureCore;
using OpenCvSharp;
using SharpDX;
using SharpDX.Direct3D11;

namespace WPFCaptureSample.Helper
{
    public static class Texture2DExtensions
    {
        public static Mat ToMat(this Texture2D texture2D, Mat existing = null)
        {
            Mat mat;
            return TextureHelper.WithCopiedTexture(texture2D, (texture, surface, dataBox, device) =>
            {
                //var map = surface.Map(SharpDX.DXGI.MapFlags.Read, out var dataStream);
                int width = surface.Description.Width;
                int height = surface.Description.Height;
                var openCvSize = new OpenCvSharp.Size(width, height);
                mat = existing != null ? existing.Resize(openCvSize) : new Mat(openCvSize, MatType.CV_8UC4, new Scalar(0));
                int channels = mat.Channels();
                int bitsPerPixel = ((int)PixelFormat.Format32bppRgb & 0xff00) >> 8;
                int bytesPerPixel = (bitsPerPixel + 7) / 8;
                int stride = channels * ((width * bytesPerPixel + 3) / channels);


                IntPtr dataBoxPointer = dataBox.DataPointer;
                IntPtr bitmapDataPointer = mat.Data;
                for (var y = 0; y < height; y++)
                {
                    Utilities.CopyMemory(bitmapDataPointer, dataBoxPointer, width * channels);
                    dataBoxPointer = IntPtr.Add(dataBoxPointer, dataBox.RowPitch);
                    bitmapDataPointer = IntPtr.Add(bitmapDataPointer, stride);
                }

                return mat;
            });
        }
    }
}