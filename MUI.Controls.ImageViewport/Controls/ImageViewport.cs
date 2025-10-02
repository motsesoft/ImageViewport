using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts;

namespace MUI.Controls.ImageViewport
{
    [TemplatePart(Name = PART_SurfaceHost, Type = typeof(LayeredSurfaceHost))]
    public class ImageViewport : Control
    {
        public const string PART_SurfaceHost = "PART_SurfaceHost";

        static ImageViewport()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewport),
                new FrameworkPropertyMetadata(typeof(ImageViewport)));
            FocusableProperty.OverrideMetadata(typeof(ImageViewport), new FrameworkPropertyMetadata(true));
            SnapsToDevicePixelsProperty.OverrideMetadata(typeof(ImageViewport), new FrameworkPropertyMetadata(true));
            UseLayoutRoundingProperty.OverrideMetadata(typeof(ImageViewport), new FrameworkPropertyMetadata(true));
        }

        // ===== 依赖属性 =====
        public static readonly DependencyProperty ViewMatrixProperty =
            DependencyProperty.Register(nameof(ViewMatrix), typeof(Matrix), typeof(ImageViewport),
                new FrameworkPropertyMetadata(Matrix.Identity, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnViewMatrixChanged));

        public Matrix ViewMatrix
        {
            get => (Matrix)GetValue(ViewMatrixProperty);
            set => SetValue(ViewMatrixProperty, value);
        }

        public static readonly DependencyProperty EnableBuiltInInteractionProperty =
            DependencyProperty.Register(nameof(EnableBuiltInInteraction), typeof(bool), typeof(ImageViewport),
                new PropertyMetadata(true));

        public bool EnableBuiltInInteraction
        {
            get => (bool)GetValue(EnableBuiltInInteractionProperty);
            set => SetValue(EnableBuiltInInteractionProperty, value);
        }

        // ===== 内部状态 =====
        LayeredSurfaceHost? _host;
        Matrix _inv; bool _hasInv;
        bool _isPanning;
        Point _panLastScreen; // ← 只保留“上一次屏幕坐标”
        Point _panAnchorScreen, _panAnchorWorld;

        ViewportFacade? _facade;
        public IViewportFacade Facade => _facade ??= new ViewportFacade(this);

        // ===== 生命周期 =====
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _host = GetTemplateChild(PART_SurfaceHost) as LayeredSurfaceHost;
            if (_host == null) return;

            _host.SetupTransforms(new MatrixTransform(ViewMatrix), Transform.Identity);
            SizeChanged -= OnViewportSizeChanged;
            SizeChanged += OnViewportSizeChanged;

            UpdateInverse();
            _host.InvalidateVisual();
        }

        static void OnViewMatrixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vp = (ImageViewport)d;
            vp.UpdateInverse();
            vp._host?.UpdateWorldLayerTransform(new MatrixTransform(vp.ViewMatrix));
            vp._facade?.RaiseViewMatrixChanged();
            vp._host?.InvalidateVisual();
        }

        void OnViewportSizeChanged(object s, SizeChangedEventArgs e)
        {
            _facade?.RaiseSurfacesInvalidated();
            _host?.InvalidateVisual();
        }

        void UpdateInverse()
        {
            _inv = ViewMatrix;
            _hasInv = _inv.HasInverse;
            if (_hasInv) _inv.Invert();
        }

        Rect GetViewportBounds()
        {
            var s = RenderSize;
            return s.IsEmpty || s.Width <= 0 || s.Height <= 0 ? Rect.Empty : new Rect(new Point(0, 0), s);
        }

        // 1) 可视世界矩形
        Rect GetWorldVisibleBounds()
        {
            var vp = GetViewportBounds();
            if (!_hasInv || vp.IsEmpty) return Rect.Empty;
            return Rect.Transform(vp, _inv);
        }

        // ===== 坐标变换 =====
        public Point ScreenToWorld(Point p) => _hasInv ? _inv.Transform(p) : p;
        public Point WorldToScreen(Point p) => ViewMatrix.Transform(p);
        public Rect ScreenToWorld(Rect r) => _hasInv ? Rect.Transform(r, _inv) : r;
        public Rect WorldToScreen(Rect r) => Rect.Transform(r, ViewMatrix);

        // ===== 简易交互（滚轮缩放、左键平移，可关闭） =====
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!EnableBuiltInInteraction || _host == null) return;

            var pScreen = e.GetPosition(this);
            var w = ScreenToWorld(pScreen);               // 鼠标对应的世界点（图像像素）
            var z = e.Delta > 0 ? 1.2 : 1.0 / 1.2;        // 缩放因子

            // 取当前矩阵的缩放与平移（假设没有旋转/斜切；本控件也只做缩放+平移）
            var m = ViewMatrix;
            var sx = m.M11;
            var sy = m.M22;
            var tx = m.OffsetX;
            var ty = m.OffsetY;

            // 新的缩放
            var sx2 = sx * z;
            var sy2 = sy * z;

            // 关键：让世界点 w 在缩放后仍映射到原来的屏幕位置
            // 原屏幕坐标: s = (sx*w.X + tx, sy*w.Y + ty)
            // 要求新坐标 s' = (sx2*w.X + tx2, sy2*w.Y + ty2) == s
            // 解得：
            var tx2 = tx + sx * w.X - sx2 * w.X;   // = tx + sx*w.X*(1 - z)
            var ty2 = ty + sy * w.Y - sy2 * w.Y;   // = ty + sy*w.Y*(1 - z)

            m.M11 = sx2; m.M22 = sy2;
            m.OffsetX = tx2; m.OffsetY = ty2;

            ViewMatrix = m;
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (!EnableBuiltInInteraction) return;

            Focus();
            _isPanning = true;
            _panAnchorWorld = ScreenToWorld(e.GetPosition(this)); // 记录世界锚点
            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!EnableBuiltInInteraction || !_isPanning) return;

            var curScreen = e.GetPosition(this);
            var anchorScreenNow = WorldToScreen(_panAnchorWorld); // 受当前矩阵影响的锚点屏幕位置
            var dx = curScreen.X - anchorScreenNow.X;
            var dy = curScreen.Y - anchorScreenNow.Y;

            // 前乘屏幕平移，把锚点精确对齐到当前鼠标位置
            var t = new Matrix(1, 0, 0, 1, dx, dy);
            ViewMatrix = t * ViewMatrix;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!EnableBuiltInInteraction) return;

            if (_isPanning)
            {
                _isPanning = false;
                ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        // ===== Facade（内部类） =====
        sealed class ViewportFacade : IViewportFacade
        {
            readonly ImageViewport _owner;
            internal ViewportFacade(ImageViewport owner) { _owner = owner; }

            public Matrix ViewMatrix => _owner.ViewMatrix;
            public Matrix InverseViewMatrix => _owner._inv;

            public event EventHandler ViewMatrixChanged;
            internal void RaiseViewMatrixChanged() => ViewMatrixChanged?.Invoke(this, EventArgs.Empty);

            public DrawingContext Open(LayerKind layer, bool clear = true)
                => _owner._host?.OpenExternal(layer, clear);

            public Rect ViewportBounds => _owner.GetViewportBounds();
            public Rect WorldVisibleBounds => _owner.GetWorldVisibleBounds();

            public event EventHandler SurfacesInvalidated;
            internal void RaiseSurfacesInvalidated() => SurfacesInvalidated?.Invoke(this, EventArgs.Empty);

            public void Invalidate() => _owner._host?.InvalidateVisual();
            public void Clear(LayerKind layer) => _owner._host?.ClearExternal(layer);
            public void ClearAll() => _owner._host?.ClearAllExternal();

            public Point ScreenToWorld(Point p) => _owner.ScreenToWorld(p);
            public Point WorldToScreen(Point p) => _owner.WorldToScreen(p);
            public Rect ScreenToWorld(Rect r) => _owner.ScreenToWorld(r);
            public Rect WorldToScreen(Rect r) => _owner.WorldToScreen(r);
        }
    }
}
