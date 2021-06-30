using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = System.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;


namespace CaptureCore
{
    public static class TextureHelper
    {
    
        public static Texture2D CreateTexture2DFrombytes(Device device, byte[] RawData, int width, int height)
        {
            Texture2DDescription desc;
            desc.Width = width;
            desc.Height = height;
            desc.ArraySize = 1;
            desc.BindFlags = BindFlags.ShaderResource;
            desc.Usage = ResourceUsage.Immutable;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.Format = Format.B8G8R8A8_UNorm;
            desc.MipLevels = 1;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            DataStream s = DataStream.Create(RawData, true, true);
            DataRectangle rect = new DataRectangle(s.DataPointer, width * 4);
            Texture2D t2D = new Texture2D(device, desc, rect);
            return t2D;
        }

        public static byte[] ToBytes(this Texture2D txt2d)
        {
            return WithCopiedTexture(txt2d, (texture, surface, dataBox, device) =>
            {
                byte[] result;
                int width = surface.Description.Width;
                int height = surface.Description.Height;

                // ##  Test 1
                surface.Map(SharpDX.DXGI.MapFlags.Read, out var dataStream);
                // var lines = (int)(dataStream.Length / map.Pitch);
                // data = new byte[surface.Description.Width * surface.Description.Height * 4];
                result = new byte[dataStream.Length];

                using (var memoryStream = new MemoryStream(result))
                {
                    dataStream.CopyTo(memoryStream);
                }

                dataStream.Dispose();
                surface.Unmap();
                return result;

                // ## Test 2
                int channels = 4;
                int bitsPerPixel = ((int)PixelFormat.Format32bppRgb & 0xff00) >> 8;
                int bytesPerPixel = (bitsPerPixel + 7) / 8;
                int stride = channels * ((width * bytesPerPixel + 3) / channels);

                result = new byte[width * height* channels];
                IntPtr dataBoxPointer = dataBox.DataPointer;
                for (var y = 0; y < height; y++)
                {
                    Marshal.Copy(dataBox.DataPointer, result, 0, width * channels);
                    dataBoxPointer = IntPtr.Add(dataBoxPointer, dataBox.RowPitch);
                }
                return result;


                // ## Test 3
                IntPtr ptr = dataBox.DataPointer;
                var psize = width * height * 4;
                result = new byte[psize];
                Marshal.Copy(ptr, result, 0, psize);
                return result;
            });
        }


        public static Bitmap ToBitmap(this Texture2D texture2D)
        {
            return WithCopiedTexture(texture2D, (texture2D1, surface, dataBox, device) =>
            {
                int width = surface.Description.Width;
                int height = surface.Description.Height;
                Rectangle bounds = new Rectangle(System.Drawing.Point.Empty, new System.Drawing.Size(width, height));
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
                int channels = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                BitmapData bitmapData = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                IntPtr dataBoxPointer = dataBox.DataPointer;
                IntPtr bitmapDataPointer = bitmapData.Scan0;
                for (Int32 y = 0; y < height; y++)
                {
                    Utilities.CopyMemory(bitmapDataPointer, dataBoxPointer, width * channels);
                    dataBoxPointer = IntPtr.Add(dataBoxPointer, dataBox.RowPitch);
                    bitmapDataPointer = IntPtr.Add(bitmapDataPointer, bitmapData.Stride);
                }

                bitmap.UnlockBits(bitmapData);

                return bitmap;
            });
        }

        public static T WithCopiedTexture<T>(Texture2D texture2D, Func<Texture2D, Surface, DataBox, Device, T> action)
        {
            var device = texture2D.Device;
            var desc = texture2D.Description;
            desc.CpuAccessFlags = CpuAccessFlags.Read;
            desc.Usage = ResourceUsage.Staging;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.BindFlags = BindFlags.None;

            using (var texture = new Texture2D(device, desc))
            {
                device.ImmediateContext.CopyResource(texture2D, texture);

                using (var surface = texture.QueryInterface<Surface>())
                {
                    try
                    {
                        DataBox dataBox = device.ImmediateContext.MapSubresource(texture, 0, MapMode.Read, MapFlags.None);
                        return action(texture, surface, dataBox, device);
                    }
                    finally
                    {
                        device.ImmediateContext.UnmapSubresource(texture, 0);
                    }

                }

            }
        }

    }
}