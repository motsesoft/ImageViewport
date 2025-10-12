using System.Windows;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Surfaces;
using Samples.ImageViewport.DemoApp.ViewModels;

namespace Samples.ImageViewport.DemoApp.Overlays
{
    public sealed class SelectionOverlayRenderer : ISurfaceRenderer
    {
        private readonly ViewportViewModel _vm;
        public SelectionOverlayRenderer(ViewportViewModel vm) { _vm = vm; }

        public void Render(DrawingContext dc, Rect windowRect, ViewportInfo view, IViewportTransforms transforms, object[] surfaces)
        {
            if (!_vm.SelectionActive) return;
            var r = _vm.SelectionWindowRect;
            if (r.Width <= 0 || r.Height <= 0) return;

            var rect = new Rect(r.X, r.Y, r.Width, r.Height);
            var fill = new SolidColorBrush(Color.FromArgb(50, 0, 120, 215));
            var pen  = new Pen(new SolidColorBrush(Color.FromArgb(180, 0, 120, 215)), 1);
            dc.DrawRectangle(fill, pen, rect);
        }
    }
}