using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using Alturos.Yolo;
using Alturos.Yolo.Model;
using CaptureCore;
using SharpDX.Direct3D11;
using WPFCaptureSample.Helper;

namespace WPFCaptureSample
{
    public class DetectedImage : IDetectedImage
    {
        public DetectedImage(Texture2D texture2D, YoloWrapper yolo)
        {
            Bitmap = texture2D.ToBitmap();
            Detections = yolo.Detect(Bitmap) ?? Enumerable.Empty<YoloItem>();
            Bitmap.PaintDetections(Detections);
        }

        public Bitmap Bitmap { get; set; }
        public IEnumerable<YoloItem> Detections { get; }

        public ImageSource ToImageSource()
        {
            return Bitmap.ToImageSource();
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
        }
    }

    public interface IDetectedImage : IDisposable
    {
        IEnumerable<YoloItem> Detections { get; }
        ImageSource ToImageSource();
    }
}