using System.Windows;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace MUI.Controls.ImageViewport.Surfaces
{
    public sealed class CrosshairSurfaceRenderer : ISurfaceRenderer
    {
        public Brush Brush { get; set; } = Brushes.Yellow;
        public double Thickness { get; set; } = 1.0;

        public void Render(DrawingContext dc, Rect windowRect, ViewportInfo view, IViewportTransforms tf, object[] surfaces)
        {
            var center = new Point(windowRect.Width / 2.0, windowRect.Height / 2.0);
            var pen = new Pen(Brush, Thickness);
            dc.DrawLine(pen, new Point(0, center.Y), new Point(windowRect.Width, center.Y));
            dc.DrawLine(pen, new Point(center.X, 0), new Point(center.X, windowRect.Height));
        }
    }
}