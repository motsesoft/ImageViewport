using System.Windows.Media;

using ImageViewport.DemoApp.ViewModels;

using MUI.Controls.ImageViewport.Contracts.Surfaces;
using MUI.Controls.ImageViewport.Surfaces.Primitives;

namespace Samples.ImageViewport.DemoApp.Overlays
{
    /// <summary>Delegates to RulerSurfaceRenderer but pulls Units/Mode from VM each render.</summary>
    public sealed class RulerBindingRenderer : ISurfaceRenderer
    {
        private readonly ViewportViewModel _vm;
        private readonly RulerSurfaceRenderer _inner = new RulerSurfaceRenderer();
        public RulerBindingRenderer(ViewportViewModel vm) { _vm = vm; }

        public SurfaceMode TransformMode => throw new System.NotImplementedException();

        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            _inner.Units = _vm.RulerUnits;
            _inner.Mode = _vm.RulerTickMode;
            _inner.Render(dc, ctx);
        }
    }
}
