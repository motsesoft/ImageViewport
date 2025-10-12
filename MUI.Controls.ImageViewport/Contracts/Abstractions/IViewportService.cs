namespace MUI.Controls.ImageViewport.Contracts.Abstractions
{
    public interface IViewportService
    {
        ViewportInfo Snapshot();
        void ZoomAtWindowPx(double scaleFactor, PxPoint windowPx);
        void PanWindowPx(double dx, double dy);
        void FitImageRect(PxRect imagePxRect);
        void SetWindowSize(PxSize size);
        void SetDpi(double x, double y);
    }
}
