using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace MUI.Controls.ImageViewport.Facade
{
    public interface IInteractiveSurface : ISurfaceRenderer
    {
        IInputRouter? Router { get; }
    }

    public interface IInputPrioritizable
    {
        int Priority { get; }
    }
}