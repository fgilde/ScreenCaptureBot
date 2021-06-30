//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using System;
using System.Numerics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.UI.Composition;

namespace CaptureCore
{
    public class CompositionCaptureApplication : IDisposable
    {
        private Compositor compositor;
        private ContainerVisual root;

        public SpriteVisual Content;
        private CompositionSurfaceBrush brush;

        private IDirect3DDevice device;
        public ScreenCapture Capture;

        public CompositionCaptureApplication(Compositor c)
        {
            compositor = c;
            device = Direct3D11Helper.CreateDevice();

            // Setup the root.
            root = compositor.CreateContainerVisual();
            root.RelativeSizeAdjustment = Vector2.One;

            // Setup the Content.
            brush = compositor.CreateSurfaceBrush();
            brush.HorizontalAlignmentRatio = 0.5f;
            brush.VerticalAlignmentRatio = 0.5f;
            brush.Stretch = CompositionStretch.Uniform;

            var shadow = compositor.CreateDropShadow();
            shadow.Mask = brush;

            Content = compositor.CreateSpriteVisual();
            Content.AnchorPoint = new Vector2(0.5f);
            Content.RelativeOffsetAdjustment = new Vector3(0.5f, 0.5f, 0);
            Content.RelativeSizeAdjustment = Vector2.One;

            Content.Size = new Vector2(-80, -80);
            Content.Brush = brush;
            Content.Shadow = shadow;
            root.Children.InsertAtTop(Content);
        }

        public Visual Visual => root;

        public void Dispose()
        {
            StopCapture();
            compositor = null;
            root.Dispose();
            Content.Dispose();
            brush.Dispose();
            device.Dispose();
        }

        public void StartCaptureFromItem(GraphicsCaptureItem item)
        {
            if (item == null)
                return;
            StopCapture();
            Capture = new ScreenCapture(device, item);

            var surface = Capture.CreateSurface(compositor);
            if (brush != null)
                brush.Surface = surface;

            Capture.StartCapture();
        }

        public void StopCapture()
        {
            Capture?.Dispose();
            if (brush != null)
                brush.Surface = null;
        }
    }
}
