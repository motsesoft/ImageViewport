
using System;
using System.Windows;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace Samples.ImageViewport.DemoApp.Overlays
{
    /// <summary>Wrap another ISurfaceRenderer and render it only when predicate() returns true.</summary>
    public sealed class ConditionalRenderer : ISurfaceRenderer
    {
        private readonly ISurfaceRenderer _inner;
        private readonly Func<bool> _predicate;
        public ConditionalRenderer(ISurfaceRenderer inner, Func<bool> predicate){ _inner = inner; _predicate = predicate; }
        public void Render(DrawingContext dc, Rect windowRect, ViewportInfo view, IViewportTransforms transforms, object[] surfaces)
        {
            if (_predicate()) _inner.Render(dc, windowRect, view, transforms, surfaces);
        }
    }
}
