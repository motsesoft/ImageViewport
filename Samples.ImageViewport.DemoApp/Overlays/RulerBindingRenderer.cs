
using System.Windows;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Surfaces;
using MUI.Controls.ImageViewport.Surfaces;
using Samples.ImageViewport.DemoApp.ViewModels;

namespace Samples.ImageViewport.DemoApp.Overlays
{
    /// <summary>Delegates to RulerSurfaceRenderer but pulls Units/Mode from VM each render.</summary>
    public sealed class RulerBindingRenderer : ISurfaceRenderer
    {
        private readonly ViewportViewModel _vm;
        private readonly RulerSurfaceRenderer _inner = new RulerSurfaceRenderer();
        public RulerBindingRenderer(ViewportViewModel vm){ _vm = vm; }
        public void Render(DrawingContext dc, Rect windowRect, ViewportInfo view, IViewportTransforms tf, object[] surfaces)
        {
            _inner.Units = _vm.RulerUnits;
            _inner.Mode = _vm.RulerTickMode;
            _inner.Render(dc, windowRect, view, tf, surfaces);
        }
    }
}
