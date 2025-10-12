using MUI.Controls.ImageViewport.Contracts.Facade;
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Contracts.Surfaces;
using MUI.Controls.ImageViewport.Handlers;
using MUI.Controls.ImageViewport.Surfaces;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using Samples.ImageViewport.DemoApp.ViewModels;
using Samples.ImageViewport.DemoApp.Overlays;
using Samples.ImageViewport.DemoApp.Adapters;
using System.Windows;

namespace Samples.ImageViewport.DemoApp
{
    public sealed class DemoFacade : IViewportFacade
    {
        public ISurfaceRenderer Renderer { get; }
        public IInputRouter? InputRouter { get; }
        public IContextMenuProvider? ContextMenu { get; }

        public DemoFacade(ViewportViewModel vm, System.Func<PxRect, PxRect> winToImgRect)
        {
            ImageSource img = (ImageSource)(Application.Current.TryFindResource("DemoImage") ?? new DrawingImage());
            if (img is DrawingImage) // fallback to pack URI if resource missing
            {
                try { img = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("pack://application:,,,/Samples.ImageViewport.DemoApp;component/Assets/demo_checker.png")); }
                catch { }
            }
            var imageRenderer = new Overlays.ImageSurfaceRenderer { Source = img, ImageRectPx = new PxRect(0,0,0,0) };

            Renderer = new CompositeRenderer(new ISurfaceRenderer[]
            {
                imageRenderer,
                new ConditionalRenderer(new GridSurfaceRenderer(), () => vm.ShowGrid),
                new ConditionalRenderer(new RulerBindingRenderer(vm), () => vm.ShowRuler),
                new ConditionalRenderer(new SelectionOverlayRenderer(vm), () => vm.ShowSelectionOverlay),
                new ConditionalRenderer(new MeasurementOverlayRenderer(vm), () => vm.ShowMeasurementOverlay),
                new ConditionalRenderer(new HudOverlayRenderer(vm), () => vm.ShowHud),
                new ConditionalRenderer(new CrosshairSurfaceRenderer(), () => vm.ShowCrosshair)
            });

            var panCfg = new VmPanZoomConfig(vm);
            var selSink = new VmSelectionSink(vm, winToImgRect);
            InputRouter = new CompositeInputRouter(new IInputRouter[]
            {
                new PanZoomHandler(panCfg),
                new BoxSelectHandler(selSink, new BoxSelectOptions{ ActivationModifier = System.Windows.Input.ModifierKeys.Shift }),
                new MouseTrackRouter(vm)
            });
            ContextMenu = new DemoContextMenuProvider(new MainViewModel(){ /* simple menu VM */});
        }
    }

    public sealed class CompositeRenderer : ISurfaceRenderer
    {
        private readonly ISurfaceRenderer[] _renderers;
        public CompositeRenderer(ISurfaceRenderer[] renderers) { _renderers = renderers; }

        public void Render(DrawingContext dc,
                           Rect windowRect,
                           ViewportInfo view,
                           IViewportTransforms transforms,
                           object[] surfaces)
        {
            foreach (var r in _renderers)
                r.Render(dc, windowRect, view, transforms, surfaces);
        }
    }

    public sealed class CompositeInputRouter : IInputRouter
    {
        private readonly IInputRouter[] _routers;
        public CompositeInputRouter(IInputRouter[] routers){ _routers = routers; }
        public bool OnLeftDown(object sender, PointerEvent p){ bool h=false; foreach(var r in _routers) h = r.OnLeftDown(sender,p) || h; return h; }
        public bool OnLeftUp(object sender, PointerEvent p){ bool h=false; foreach(var r in _routers) h = r.OnLeftUp(sender,p) || h; return h; }
        public bool OnRightDown(object sender, PointerEvent p){ bool h=false; foreach(var r in _routers) h = r.OnRightDown(sender,p) || h; return h; }
        public bool OnRightUp(object sender, PointerEvent p){ bool h=false; foreach(var r in _routers) h = r.OnRightUp(sender,p) || h; return h; }
        public bool OnMove(object sender, PointerEvent p){ bool h=false; foreach(var r in _routers) h = r.OnMove(sender,p) || h; return h; }
        public bool OnWheel(object sender, PointerEvent p){ bool h=false; foreach(var r in _routers) h = r.OnWheel(sender,p) || h; return h; }
        public bool OnMouseDown(object sender, PointerEvent p){ bool h=false; foreach(var r in _routers) h = r.OnMouseDown(sender,p) || h; return h; }
        public bool OnMouseUp(object sender, PointerEvent p){ bool h=false; foreach(var r in _routers) h = r.OnMouseUp(sender,p) || h; return h; }
    }
}