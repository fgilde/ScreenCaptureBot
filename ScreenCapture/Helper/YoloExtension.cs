using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Alturos.Yolo;
using Alturos.Yolo.Model;
using CaptureCore;
using ImageProcessor.Imaging.Colors;
using OpenCvSharp;

namespace WPFCaptureSample.Helper
{
    public static class YoloExtension
    {
        public static Task<IEnumerable<YoloItem>> DetectAsync(this YoloWrapper yolo, Bitmap img)
        {
            return Task.Run(() => yolo.Detect(img));
        }



        public static IEnumerable<YoloItem> Detect(this YoloWrapper yolo,  Bitmap img)
        {
            // var size2 = img.Height * img.Width * 4;
            // var nativePointer = (IntPtr)img.GetType().GetField("nativeImage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(img);
            int size;
            //var res = yolo.Detect(nativePointer, img.Height * img.Width * 4);
            BitmapData bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
            size = Math.Abs(bmpData.Stride) * bmpData.Height;

            var bytes = img.ToBytes();
            IntPtr num1 = Marshal.AllocHGlobal(Marshal.SizeOf<byte>(bytes[0]) * bytes.Length);
            Marshal.Copy(bytes, 0, num1, bytes.Length);

            var res2 = yolo.Detect(num1, bytes.Length);

            img.UnlockBits(bmpData);
            return res2;

        }

        public static IEnumerable<YoloItem> Detect(this YoloWrapper yolo, Mat mat)
        {
            return yolo.Detect(mat.ToBytes());
            //int size;
            ////// var size = mat.Width * mat.Height * mat.Channels();
            ////var size = ImageHelper.GetBmpMemorySize(mat.Width, mat.Height, false);
            ////var res =  yolo.Detect(mat.Data, size);
            //var res = yolo.Detect(ImageHelper.CreateBmp(mat.Data, mat.Width, mat.Height, out size), size);
            //return res;
        }
    }
}