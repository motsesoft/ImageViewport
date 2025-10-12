namespace MUI.Controls.ImageViewport.Contracts.Abstractions
{
    public sealed class ViewportInfo
    {
        public PxSize WindowPixelSize { get; init; }
        public PxRect ViewportRectInImage { get; init; }
        public double Scale { get; init; }
        public double DpiScaleX { get; init; } = 1.0;
        public double DpiScaleY { get; init; } = 1.0;
    }
}
