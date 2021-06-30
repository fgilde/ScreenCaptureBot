using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Foundation.Metadata;
using Windows.Graphics.Capture;
using CaptureCore;
using SharpDX.Direct3D11;

namespace WPFCaptureSample.Controls
{
    /// <summary>
    /// Interaction logic for GraphicDeviceSelector.xaml
    /// </summary>
    public partial class GraphicDeviceSelector : UserControl
    {

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            "IconBrush", typeof(Brush), typeof(GraphicDeviceSelector), new PropertyMetadata(default(Brush)));

        public Brush IconBrush
        {
            get { return (Brush) GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }

        public event EventHandler<GraphicDeviceSelectorEventArgs> Selected;
        public event EventHandler<CameraInfo> CameraSelected;


        public GraphicDeviceSelector()
        {
            InitializeComponent();
            DataContext = this;
#if DEBUG
            // Force graphicscapture.dll to load.
            var picker = new GraphicsCapturePicker();
#endif
        }


        private void ProcessBtnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.ContextMenu != null)
                btn.ContextMenu.IsOpen = true;
            else
            {
                btn.ContextMenu = new ContextMenu();
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.PlacementTarget = btn;
                var primary = new MenuItem { Header = "Select Application..." };
                primary.Click += (o, args) => OnSelect();
                btn.ContextMenu.Items.Add(primary);
                btn.ContextMenu.Items.Add(new Separator());
                foreach (var process in WindowEnumerationHelper.RecordableProcesses())
                {
                    var menuItem = new MenuItem() { Header = process.MainWindowTitle };
                    menuItem.Click += (o, args) => OnSelect(process);
                    btn.ContextMenu.Items.Add(menuItem);
                }

                btn.ContextMenu.IsOpen = true;
            }
        }

        private void MonitorBtnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.ContextMenu != null)
                btn.ContextMenu.IsOpen = true;
            else
            {
                btn.ContextMenu = new ContextMenu();
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.PlacementTarget = btn;
                var primary = new MenuItem { Header = "Use Primary Monitor" };
                primary.Click += (o, args) => OnSelect(MonitorEnumerationHelper.GetMonitors().FirstOrDefault(info => info.IsPrimary));
                btn.ContextMenu.Items.Add(primary);
                btn.ContextMenu.Items.Add(new Separator());
                foreach (var monitor in MonitorEnumerationHelper.GetMonitors())
                {
                    var menuItem = new MenuItem() {Header = monitor.DeviceName};
                    menuItem.Click += (o, args) => OnSelect(monitor);
                    btn.ContextMenu.Items.Add(menuItem);
                }

                btn.ContextMenu.IsOpen = true;
            }
        }

        private void CameraBtnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.ContextMenu != null)
                btn.ContextMenu.IsOpen = true;
            else
            {
                btn.ContextMenu = new ContextMenu();
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.PlacementTarget = btn;
                foreach (var cam in CameraEnumerationHelper.EnumerateCameras())
                {
                    var menuItem = new MenuItem() { Header = cam.Name };
                    menuItem.Click += (o, args) => OnSelect(cam);
                    btn.ContextMenu.Items.Add(menuItem);
                }

                btn.ContextMenu.IsOpen = true;
            }
        }

        private void OnSelect(CameraInfo cam)
        {
            CameraSelected?.Invoke(this, cam);
        }

        private async void OnSelect()
        {
            var picker = new GraphicsCapturePicker();
            picker.SetWindow(new WindowInteropHelper(Application.Current.MainWindow).Handle);
            GraphicsCaptureItem item = await picker.PickSingleItemAsync();
            if (item != null) 
                OnSelected(new GraphicDeviceSelectorEventArgs(item, IntPtr.Zero,  null));
        }
        private void OnSelect(Process process)
        {
            if (process != null)
                OnSelected(new GraphicDeviceSelectorEventArgs(CaptureHelper.CreateItemForWindow(process.MainWindowHandle), process.MainWindowHandle, process));
        }

        private void OnSelect(MonitorInfo monitor)
        {
            if (monitor != null)
                OnSelected(new GraphicDeviceSelectorEventArgs(CaptureHelper.CreateItemForMonitor(monitor.Hmon), monitor.Hmon, monitor));
        }

        protected virtual void OnSelected(GraphicDeviceSelectorEventArgs e)
        {
            Selected?.Invoke(this, e);
        }

    }

    public class GraphicDeviceSelectorEventArgs 
    {
        public GraphicDeviceSelectorEventArgs(GraphicsCaptureItem item, IntPtr handle, object originalItem)
        {
            Item = item;
            Handle = handle;
            OriginalItem = originalItem;
        }

        public GraphicsCaptureItem Item { get; set; }
        public IntPtr Handle { get; set; }
        public object OriginalItem { get; set; }
    }
}
