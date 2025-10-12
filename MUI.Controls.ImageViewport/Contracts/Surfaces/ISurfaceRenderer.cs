using System.Windows.Media;

namespace MUI.Controls.ImageViewport.Contracts.Surfaces
{
    public interface ISurfaceRenderer
    {
        SurfaceMode TransformMode { get; }
        void Render(DrawingContext dc, in SurfaceRenderContext ctx);
    }
}
