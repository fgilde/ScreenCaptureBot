using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Alturos.Yolo.Model;
using OpenCvSharp;

namespace WPFCaptureSample.Helper
{
    public static class Utils
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ToImageSource(this Bitmap bmp)
        {
            try
            {
                var handle = bmp.GetHbitmap();
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(handle);
                }
            }
            catch
            {
                return null;
            }
        }

        public static System.Windows.Rect GetAbsolutePlacement(this FrameworkElement element, bool relativeToScreen = false)
        {
            var absolutePos = element.PointToScreen(new System.Windows.Point(0, 0));
            if (relativeToScreen)
            {
                return new System.Windows.Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);
            }
            var posMW = Application.Current.MainWindow.PointToScreen(new System.Windows.Point(0, 0));
            absolutePos = new System.Windows.Point(absolutePos.X - posMW.X, absolutePos.Y - posMW.Y);
            return new System.Windows.Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);
        }

        public static Tuple<double, double> GetDpi(this Visual visual)
        {
            var presentationSource = PresentationSource.FromVisual(visual);
            double dpiX = 1.0;
            double dpiY = 1.0;
            if (presentationSource != null)
            {
                dpiX = presentationSource.CompositionTarget.TransformToDevice.M11;
                dpiY = presentationSource.CompositionTarget.TransformToDevice.M22;
            }

            return Tuple.Create(dpiX, dpiY);
        }

        public static IntPtr GetHandle(this System.Windows.Window window)
        {
            var interopWindow = new WindowInteropHelper(window);
            return interopWindow.Handle;
        }

        public static byte[] ToBytes(this Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static byte[] ToBytes(Image img, System.Drawing.Imaging.ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static Mat PaintDetections(this Mat image, IEnumerable<YoloItem> items)
        {
            if (items == null)
                return image;
            foreach (var item in items)
            {
                var x = item.X;
                var y = item.Y;
                var width = item.Width;
                var height = item.Height;
                var type = item.Type;  // class name of the object

                // draw a bounding box for the detected object
                // you can set different colors for different classes
                Cv2.Rectangle(image, new OpenCvSharp.Rect(x, y, width, height), Scalar.Red, 1);

                //thumbnailGraphic.DrawString(text, drawFont, fontBrush, atPoint);
            }
            return image;
        }

        public static Bitmap PaintDetections(this Bitmap image, IEnumerable<YoloItem> items)
        {
            if (items == null)
                return image;
            var originalImageHeight = image.Height;
            var originalImageWidth = image.Width;
            var boxColor = System.Drawing.Color.OrangeRed;
            var fgColor = System.Drawing.Color.Black;
            foreach (var box in items)
            {
                // Get Bounding Box Dimensions
                var x = (uint)Math.Max(box.X, 0);
                var y = (uint)Math.Max(box.Y, 0);
                var width = (uint)Math.Min(originalImageWidth - x, box.Width);
                var height = (uint)Math.Min(originalImageHeight - y, box.Height);


                // Bounding Box Text
                string text = $"{box.Type} ({(box.Confidence * 100):0}%)";

                using (Graphics thumbnailGraphic = Graphics.FromImage(image))
                {
                    thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
                    thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
                    thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    // Define Text Options
                    Font drawFont = new Font("Arial", 12, System.Drawing.FontStyle.Bold);
                    SizeF size = thumbnailGraphic.MeasureString(text, drawFont);
                    SolidBrush fontBrush = new SolidBrush(fgColor);
                    var atPoint = new System.Drawing.Point((int)x, (int)y - (int)size.Height - 1);

                    // Define BoundingBox options
                    var pen = new System.Drawing.Pen(boxColor, 3.2f);
                    SolidBrush colorBrush = new SolidBrush(boxColor);

                    // Draw text on image 

                    //thumbnailGraphic.FillRectangle(colorBrush, (int)x, (int)(y - size.Height - 1), (int)size.Width, (int)size.Height);
                    //thumbnailGraphic.DrawString(text, drawFont, fontBrush, atPoint);

                    // Draw bounding box on image
                    thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
                }
            }

            return image;
        }


    }
}