using System;
using System.Collections.Generic;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Abstractions.Events;
using MUI.Controls.ImageViewport.Contracts.Facade;
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Contracts.Surfaces;
using MUI.Controls.ImageViewport.Handlers.Contracts;
using MUI.Controls.ImageViewport.Handlers.Routers;
using MUI.Controls.ImageViewport.Runtime.Services;
using MUI.Controls.ImageViewport.Runtime.Transforms;
using MUI.Controls.ImageViewport.Surfaces.Primitives;

namespace MUI.Controls.ImageViewport.Defaults
{
    public sealed class DefaultImageViewportFacade : IViewportFacade, IDisposable
    {
        private readonly ImageViewport _viewport;
        private readonly ImageSurfaceRenderer _imageLayer = new();
        private readonly AutoFitHandler _autoFit = new() { Mode = AutoFitMode.Always };
        public IViewportService Service { get; } = new BuiltInViewportService();

        // ����
        private ulong _lastVersion = ulong.MaxValue;
        private IViewportTransforms? _cachedTransforms;

        public DefaultImageViewportFacade(ImageViewport viewport, IPanZoomOptions options, ImageSource? source)
        {
            _viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
            _imageLayer.Source = source;

            Surfaces = [_imageLayer];
            InputRouter = new PanZoomHandler(options);
            ContextMenu = null;

            // ��ʼ�� current & ����
            UpdateCache(_viewport.CurrentOrEmpty());
            if (Service is IViewportObservable obs)
                obs.ViewportChanged += OnViewportChanged;

            _viewport.WindowSizeChanged += OnWindowSizeChanged;
        }

        public IEnumerable<ISurfaceRenderer> Surfaces { get; }
        public IInputRouter? InputRouter { get; }
        public IContextMenuProvider? ContextMenu { get; }
        public ViewportInfo Current => Service.Current;

        public IViewportTransforms GetTransforms(in ViewportInfo info)
        {
            if (_cachedTransforms is null || _lastVersion != info.Version)
            {
                _cachedTransforms = new BuiltInViewportTransforms(info);
                _lastVersion = info.Version;
            }
            return _cachedTransforms;
        }

        public void SetSource(ImageSource? src)
        {
            _imageLayer.Source = src;
            if (src is System.Windows.Media.Imaging.BitmapSource bs)
            {
                var rect = new PxRect(0, 0, bs.PixelWidth, bs.PixelHeight);
                _autoFit.SetCachedRect(rect);
                _viewport.FitImageRect(rect);
            }
        }

        public void SetAutoFitMode(AutoFitMode mode) => _autoFit.Mode = mode;

        private void OnViewportChanged(object? sender, ViewportInfo e) => UpdateCache(e);
        private void OnWindowSizeChanged(object? sender, WindowPixelSizeChangedEventArgs e)
        {
            if (_autoFit.ShouldRefitOnWindowSizeChange(e.NewSize, out var rect) && rect.HasValue)
            {
                _viewport.FitImageRect(rect.Value);
            }
        }
        private void UpdateCache(ViewportInfo info)
        {
            if (info.Version == _lastVersion && _cachedTransforms is not null) return;

            _cachedTransforms = new BuiltInViewportTransforms(info); // �ؽ�
            _lastVersion = info.Version;
        }

        public void Dispose()
        {
            if (Service is IViewportObservable obs)
            {
                obs.ViewportChanged -= OnViewportChanged;
            }

            _viewport.WindowSizeChanged -= OnWindowSizeChanged;
        }
    }
}