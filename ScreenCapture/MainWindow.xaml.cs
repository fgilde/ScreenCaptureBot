using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Windows.Graphics.Capture;
using Windows.UI.Composition;
using Windows.UI.Xaml.Media;
using Alturos.Yolo;
using Alturos.Yolo.Model;
using CaptureCore;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SharpDX.Direct3D11;
using SimWinInput;
using WPFCaptureSample.Controls;
using WPFCaptureSample.Helper;
using CompositionTarget = Windows.UI.Composition.CompositionTarget;
using ContainerVisual = Windows.UI.Composition.ContainerVisual;
using Window = System.Windows.Window;

namespace WPFCaptureSample
{


    // MY Azure Cognitive ("https://cognitiveserviceadv.cognitiveservices.azure.com", "4d8a3b23019148a78e91fdcc029bd17f"));

    // https://aleen42.github.io/PersonalWiki/post/yolo/yolo.html
    // https://medium.com/analytics-vidhya/tensorflow-yolov4-counter-strike-global-offensive-realtime-aimbot-3ba5eb20453b
    // https://github.com/pythonlessons/TensorFlow-2.3.1-YOLOv4-CSGO-aimbot
    // https://github.com/lucylow/b00m-h3adsh0t
    // https://github.com/petercunha/Pine
    // https://www.unknowncheats.me/forum/counterstrike-global-offensive/262494-aimbots-3rw1ns-look-future.html

    // https://github.com/Uehwan/CSharp-Yolo-Video

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Compositor compositor;
        private CompositionTarget target;
        private ContainerVisual root;

        private CompositionCaptureApplication captureApplication;
        private YoloWrapper yoloWrapper;

        public MainWindow()
        {
            InitializeComponent();
            SimGamePad.Instance.Initialize();
            SimGamePad.Instance.PlugIn();
            Closing += (sender, args) =>
            {
                StopCapture();
                SimGamePad.Instance.Unplug();
            };
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var controlsWidth = (float)(ControlsGrid.ActualWidth * this.GetDpi().Item1);
            InitComposition(controlsWidth);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();
            YoloGrid.Items = new ObservableCollection<YoloItem>();
        }


        private void InitComposition(float controlsWidth)
        {
            var handle = this.GetHandle();
            
            // Create the compositor.
            compositor = new Compositor();

            // Create a target for the window.
            target = compositor.CreateDesktopWindowTarget(handle, false);

            // Attach the root visual.
            root = compositor.CreateContainerVisual();

            root.RelativeSizeAdjustment = Vector2.One;
            root.Size = new Vector2(-controlsWidth, 0);
            root.Offset = new Vector3(controlsWidth, 0, 0);
            target.Root = root;
            // Setup the rest of the captureApplication application.
            captureApplication = new CompositionCaptureApplication(compositor);

            if (!renderAsImage.IsChecked ?? false)
                root?.Children.InsertAtTop(captureApplication.Visual);
        }

        
        private YoloWrapper CreateYolo()
        {
            //var gpuConfig = new GpuConfig();
            var configurationDetector = new YoloConfigurationDetector();
            var config = configurationDetector.Detect();

            return new YoloWrapper(config);
        }

        private void StartCapture(GraphicsCaptureItem item)
        {
            if (item != null)
            {
                StopCapture();
                captureApplication.StartCaptureFromItem(item);
                yoloWrapper = CreateYolo();
                captureApplication.Capture.OnNewAs<DetectedImage>(OnFrame, texture2D =>  CreateDetectedImage(texture2D));
            }
        }

        private DetectedImage CreateDetectedImage(Texture2D texture2D)
        {
            return new DetectedImage(texture2D, yoloWrapper);
        }

        private void OnFrame(IDetectedImage bitmap)
        {
            YoloGrid.Items = new ObservableCollection<YoloItem>(bitmap.Detections);
            SetImage(bitmap);
        }

        private void SetImage(IDetectedImage detectedImage)
        {
            Dispatcher.Invoke(() => SetImageSource(detectedImage?.ToImageSource()));
        }

        private void SetImage(Mat b)
        {
            Dispatcher.Invoke(() => SetImageSource(b?.ToBitmapSource()));

        }

        private void SetImage(Bitmap b)
        {
            Dispatcher.Invoke(() => SetImageSource(b?.ToImageSource()));
        }

        private void SetImageSource(System.Windows.Media.ImageSource source)
        {
            if (source != null && (renderAsImage.IsChecked ?? false))
                CaptureImage.Source = source;
        }



        private void StopCapture()
        {
            cameraCaptureCancellationTokenSource?.Cancel();
            captureApplication?.StopCapture();
            cameraCaptureCancellationTokenSource = null;
            CaptureImage.Source = null;
        }

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Zoom(e.Delta > 0 ? .2 : -.2);
        }

        private void Zoom(double factor)
        {
            var b = captureApplication.Content.Brush as CompositionSurfaceBrush;

            if (factor > 1)
            {
                lastScale = b.Scale;
                lastHRatio = b.HorizontalAlignmentRatio;
                lastVRatio = b.VerticalAlignmentRatio;
                lastStretch = b.Stretch;
                lastAnchor = b.AnchorPoint;
                b.Scale = new Vector2((float)factor);
                b.VerticalAlignmentRatio = 1f;
                b.HorizontalAlignmentRatio = 1f;
                b.Stretch = CompositionStretch.UniformToFill;
                //b.AnchorPoint = new Vector2(0.5f, 0.5f);
            }
            else
            {
                b.Scale = lastScale;
                b.VerticalAlignmentRatio = lastVRatio;
                b.HorizontalAlignmentRatio = lastHRatio;
                b.Stretch = lastStretch;
                b.AnchorPoint = lastAnchor;
            }

            //captureApplication.Content.RelativeSizeAdjustment = new Vector2((float)factor);

        }

        private bool zoomed;
        private Vector2 lastScale;
        private float lastHRatio;
        private float lastVRatio;
        private CompositionStretch lastStretch;
        private Vector2 lastAnchor;

        private void Zoom_Click(object sender, RoutedEventArgs e)
        {
            //Zoom(!zoomed ? 3: 1);
            //zoomed = !zoomed;

            Thread.Sleep(2000);
            SimGamePad.Instance.State[0].LeftStickX = short.MinValue * 3 / 4;
            SimGamePad.Instance.Update(0);
            SimGamePad.Instance.Use(GamePadControl.A);
        }


        private void GraphicDeviceSelector_OnSelected(object sender, GraphicDeviceSelectorEventArgs e)
        {
            StartCapture(e.Item);
        }

        private CancellationTokenSource cameraCaptureCancellationTokenSource;
        
        private async void GraphicDeviceSelector_OnCameraSelected(object sender, CameraInfo e)
        {
            StopCapture();
            YoloWrapper yolo = CreateYolo();

            // Read a video file and run object detection over it!
            using (var videocapture = new VideoCapture(e.Index))
            {
                using (Mat imageOriginal = new Mat())
                {
                    cameraCaptureCancellationTokenSource = new CancellationTokenSource();
                    await Task.Factory.StartNew(() =>
                    {
                        while (cameraCaptureCancellationTokenSource != null && !cameraCaptureCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            // read a single frame and convert the frame into a byte array
                            videocapture.Read(imageOriginal);
                            var image = imageOriginal.Resize(new OpenCvSharp.Size(imageOriginal.Width, imageOriginal.Height));

                            // conduct object detection and display the result
                            var items = yolo.Detect(image).ToArray();
                            image.PaintDetections(items);
                            YoloGrid.Items = new ObservableCollection<YoloItem>(items);
                            // display the detection result
                            SetImage(image);
                        }
                    }, cameraCaptureCancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                    StopCapture();
                }
            }
        }

        private void RenderAsImage_OnChecked(object sender, RoutedEventArgs e)
        {
            if (renderAsImage.IsChecked ?? false)
                root?.Children.Remove(captureApplication.Visual);
            else
            {
                CaptureImage.Source = null;
                root?.Children.InsertAtTop(captureApplication.Visual);
            }
        }
    }
}
