using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace MUI.Controls.ImageViewport.Contracts.Facade
{
    public interface IViewportFacade
    {
        IEnumerable<ISurfaceRenderer> Surfaces { get; }
        IInputRouter? InputRouter { get; }
        IContextMenuProvider? ContextMenu { get; }
    }
}
