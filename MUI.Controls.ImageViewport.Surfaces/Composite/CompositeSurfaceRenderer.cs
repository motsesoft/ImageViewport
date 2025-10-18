using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace MUI.Controls.ImageViewport.Surfaces.Composite
{
    public sealed class CompositeSurfaceRenderer : ISurfaceRenderer
    {
        private readonly ISurfaceRenderer[] _renderers;
        public CompositeSurfaceRenderer(ISurfaceRenderer[] renderers)
        {
            _renderers = renderers;
        }

        public SurfaceMode TransformMode => SurfaceMode.Follow;

        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            foreach (var r in _renderers)
            {
                r.Render(dc, in ctx);
            }
        }
    }
}