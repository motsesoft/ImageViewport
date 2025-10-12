namespace MUI.Controls.ImageViewport.Contracts.Abstractions
{
    public interface IViewportObservable
    {
        event EventHandler<ViewportInfo> ViewportChanged;
    }
}
