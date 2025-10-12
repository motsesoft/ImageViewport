using System.Globalization;
using System.Windows;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Surfaces;
using Samples.ImageViewport.DemoApp.ViewModels;

namespace Samples.ImageViewport.DemoApp.Overlays
{
    public sealed class HudOverlayRenderer : ISurfaceRenderer
    {
        private readonly ViewportViewModel _vm;
        public HudOverlayRenderer(ViewportViewModel vm){ _vm = vm; }

        public void Render(DrawingContext dc, Rect windowRect, ViewportInfo view, IViewportTransforms tf, object[] surfaces)
        {
            var mouse = _vm.MouseScene;
            string m = string.Format(System.Globalization.CultureInfo.InvariantCulture, "  Mouse: ({0:0.0000}, {1:0.0000})", mouse.X, mouse.Y);
            var text = $"Scale: {view.Scale:0.###}  View: [{view.ImageViewInImagePixels.X:0},{view.ImageViewInImagePixels.Y:0} {view.ImageViewInImagePixels.Width:0}x{view.ImageViewInImagePixels.Height:0}]  Ruler: {_vm.RulerUnits}/{_vm.RulerTickMode}" + m;
            var ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                       new Typeface("Consolas"), 12, Brushes.White, 1.2);
            var bg = new SolidColorBrush(Color.FromArgb(120, 0,0,0));
            dc.DrawRectangle(bg, null, new Rect(8, windowRect.Height - ft.Height - 12, ft.Width + 8, ft.Height + 4));
            dc.DrawText(ft, new Point(12, windowRect.Height - ft.Height - 10));
        }
    }
}