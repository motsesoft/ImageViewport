using System.Globalization;
using System.Windows;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Surfaces;
using Samples.ImageViewport.DemoApp.ViewModels;

namespace Samples.ImageViewport.DemoApp.Overlays
{
    public sealed class MeasurementOverlayRenderer : ISurfaceRenderer
    {
        private readonly ViewportViewModel _vm;
        public MeasurementOverlayRenderer(ViewportViewModel vm) { _vm = vm; }

        public void Render(DrawingContext dc, Rect windowRect, ViewportInfo view, IViewportTransforms tf, object[] surfaces)
        {
            if (!_vm.HasMeasurement) return;
            var p1 = tf.ImageToWindow(_vm.MeasureP1);
            var p2 = tf.ImageToWindow(_vm.MeasureP2);

            var pen = new Pen(Brushes.Lime, 1.2);
            dc.DrawLine(pen, new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));

            var dx = _vm.MeasureP2.X - _vm.MeasureP1.X;
            var dy = _vm.MeasureP2.Y - _vm.MeasureP1.Y;
            var len = System.Math.Sqrt(dx*dx + dy*dy);
            var text = len.ToString("0.##", CultureInfo.InvariantCulture) + " px";
            var ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                       new Typeface("Consolas"), 12, Brushes.White, 1.0);
            var mid = new Point((p1.X+p2.X)/2, (p1.Y+p2.Y)/2);
            dc.DrawText(ft, new Point(mid.X + 6, mid.Y + 6));
        }
    }
}