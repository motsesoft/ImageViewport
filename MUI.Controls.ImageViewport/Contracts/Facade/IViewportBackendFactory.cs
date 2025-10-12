using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Contracts.Facade
{
    public interface IViewportBackendFactory
    {
        IViewportService CreateViewportService();
        IViewportTransforms CreateTransforms(ViewportInfo snapshot);
    }
}
