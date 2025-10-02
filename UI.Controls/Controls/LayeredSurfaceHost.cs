using System.Windows;
using System.Windows.Media;

namespace UI.Controls.ImageViewport
{
    /// <summary>
    /// 多层绘制宿主：世界域(4层) + HUD(1层)。
    /// </summary>
    public class LayeredSurfaceHost : FrameworkElement
    {
        readonly DrawingVisual _vBackground = new();
        readonly DrawingVisual _vActive = new();
        readonly DrawingVisual _vPreview = new();
        readonly DrawingVisual _vOverlayWorld = new();
        readonly DrawingVisual _vHud = new();

        readonly VisualCollection _children;

        public LayeredSurfaceHost()
        {
            _children = new VisualCollection(this)
            {
                _vBackground, _vActive, _vPreview, _vOverlayWorld, _vHud
            };
            IsHitTestVisible = false; // 仅承担渲染
        }

        public void SetupTransforms(Transform world, Transform hud)
        {
            UpdateWorldLayerTransform(world ?? Transform.Identity);
            UpdateHudLayerTransform(hud ?? Transform.Identity);
        }

        public void UpdateWorldLayerTransform(Transform world)
        {
            _vBackground.Transform = world;
            _vActive.Transform = world;
            _vPreview.Transform = world;
            _vOverlayWorld.Transform = world;
        }

        public void UpdateHudLayerTransform(Transform hud)
        {
            _vHud.Transform = hud;
        }

        public DrawingContext OpenExternal(Contracts.LayerKind layer, bool clear)
        {
            var v = GetExternalVisual(layer);
            if (v == null) return null;
            if (clear) v.RenderOpen().Close();
            return v.RenderOpen();
        }

        public void ClearExternal(Contracts.LayerKind layer)
            => GetExternalVisual(layer)?.RenderOpen().Close();

        public void ClearAllExternal()
        {
            _vBackground.RenderOpen().Close();
            _vActive.RenderOpen().Close();
            _vPreview.RenderOpen().Close();
            _vOverlayWorld.RenderOpen().Close();
            _vHud.RenderOpen().Close();
        }

        DrawingVisual GetExternalVisual(Contracts.LayerKind layer) => layer switch
        {
            Contracts.LayerKind.Background => _vBackground,
            Contracts.LayerKind.Active => _vActive,
            Contracts.LayerKind.Preview => _vPreview,
            Contracts.LayerKind.OverlayWorld => _vOverlayWorld,
            Contracts.LayerKind.Hud => _vHud,
            _ => null
        };

        protected override int VisualChildrenCount => _children.Count;
        protected override Visual GetVisualChild(int index) => _children[index];
    }
}
