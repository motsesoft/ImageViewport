using System.Windows;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Contracts.Surfaces
{
    public readonly struct SurfaceRenderContext
    {
        public Rect WindowRect { get; }
        public ViewportInfo View { get; }
        public IViewportTransforms Transforms { get; }
        public Matrix ViewportMatrix { get; }
        public Matrix ViewportMatrixInverse { get; }
        public double DpiScaleX { get; }
        public double DpiScaleY { get; }
        public IReadOnlyList<object> Payload { get; }
        public IServiceProvider? Services { get; }
        public object? Tag { get; }

        public SurfaceRenderContext(
            Rect windowRect,
            ViewportInfo view,
            IViewportTransforms transforms,
            Matrix viewportMatrix,
            Matrix viewportMatrixInverse,
            double dpiX,
            double dpiY,
            IReadOnlyList<object>? payload = null,
            IServiceProvider? services = null,
            object? tag = null)
        {
            WindowRect = windowRect;
            View = view;
            Transforms = transforms;
            ViewportMatrix = viewportMatrix;
            ViewportMatrixInverse = viewportMatrixInverse;
            DpiScaleX = dpiX;
            DpiScaleY = dpiY;
            Payload = payload ?? Array.Empty<object>();
            Services = services;
            Tag = tag;
        }
    }
}
