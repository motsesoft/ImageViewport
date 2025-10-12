using System.Windows.Controls;

namespace MUI.Controls.ImageViewport.Contracts.Input
{
    public interface IContextMenuProvider
    {
        ContextMenu? BuildContextMenu(PointerEvent p);
    }
}
