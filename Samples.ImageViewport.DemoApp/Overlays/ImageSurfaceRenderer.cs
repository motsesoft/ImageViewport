using System.Windows;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace Samples.ImageViewport.DemoApp.Overlays
{
    /// <summary>把一张 ImageSource 按图像像素(0,0,w,h) 的区域映射到窗口。</summary>
    public sealed class ImageSurfaceRenderer : ISurfaceRenderer
    {
        public ImageSource? Source { get; set; }
        public PxRect ImageRectPx { get; set; } // 图像像素空间的占位矩形，通常是 0,0,w,h

        public void Render(DrawingContext dc, Rect windowRect, ViewportInfo view, IViewportTransforms tf, object[] surfaces)
        {
            if ((ImageRectPx.Width <= 0 || ImageRectPx.Height <= 0) && Source is System.Windows.Media.Imaging.BitmapSource bs)
            {
                ImageRectPx = new PxRect(0,0, bs.PixelWidth, bs.PixelHeight);
            }
            if (Source is null || ImageRectPx.Width <= 0 || ImageRectPx.Height <= 0) return;
            var win = tf.ImageToWindow(ImageRectPx);
            var dest = new Rect(win.X, win.Y, win.Width, win.Height);
            dc.DrawImage(Source, dest);
        }
    }
}