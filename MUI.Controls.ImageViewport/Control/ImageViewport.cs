using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Abstractions.Events;
using MUI.Controls.ImageViewport.Contracts.Facade;
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Runtime.Display;
using MUI.Controls.ImageViewport.Runtime.Services;
using MUI.Controls.ImageViewport.Runtime.Transforms;

namespace MUI.Controls.ImageViewport
{
    /// <summary>
    /// 图像视口控件，支持缩放、平移、DPI 适配、窗口与图像坐标转换等功能。
    /// 提供丰富的依赖属性和事件，适用于高性能图像浏览与交互场景。
    /// </summary>
    [TemplatePart(Name = PartHost, Type = typeof(LayeredSurfaceHost))]
    public sealed class ImageViewport : Control
    {
        /// <summary>
        /// 模板部件名称：图层宿主。
        /// </summary>
        internal const string PartHost = "PART_SurfaceHost";
        LayeredSurfaceHost? _host;

        // 恢复用于输入处理的瞬时状态字段
        bool _isLeftDown;
        bool _isRightDown;
        bool _isMiddleDown;
        int _clickCountLeft;
        int _clickCountRight;

        Point? _lastMoveWindowPx;
        PxRect? _fitRectCache;

        IViewportFacade? _facade;
        IViewportBackendFactory? _backendFactory;
        IViewportService? _service;
        IInputRouter? _defaultRouter;
        readonly MatrixTransform _viewportMatrix = new(Matrix.Identity);

        static ImageViewport()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewport), new FrameworkPropertyMetadata(typeof(ImageViewport)));
        }

        /// <summary>
        /// 初始化 <see cref="ImageViewport"/> 类的新实例。
        /// </summary>
        public ImageViewport()
        {
            Focusable = true;
            SetValue(ViewportMatrixPropertyKey, _viewportMatrix);
        }

        #region 视口外观门面
        /// <summary>
        /// 获取或设置视口外观门面（用于渲染、输入、菜单等）。
        /// </summary>
        public IViewportFacade? Facade
        {
            get => (IViewportFacade?)GetValue(FacadeProperty);
            set => SetValue(FacadeProperty, value);
        }

        /// <summary>
        /// 视口外观门面依赖属性。
        /// </summary>
        public static readonly DependencyProperty FacadeProperty =
            DependencyProperty.Register(nameof(Facade), typeof(IViewportFacade),
                typeof(ImageViewport),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, _) => ((ImageViewport)d).Rebind()));
        #endregion

        #region 视口变换矩阵
        static readonly DependencyPropertyKey ViewportMatrixPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ViewportMatrix), typeof(MatrixTransform), typeof(ImageViewport),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// 视口变换矩阵（只读依赖属性）。
        /// </summary>
        public static readonly DependencyProperty ViewportMatrixProperty = ViewportMatrixPropertyKey.DependencyProperty;

        /// <summary>
        /// 获取当前视口的变换矩阵。
        /// </summary>
        public MatrixTransform ViewportMatrix => (MatrixTransform)GetValue(ViewportMatrixProperty);
        #endregion

        #region 当前视口图像坐标区域
        static readonly DependencyPropertyKey ImageViewRectPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageViewRect), typeof(PxRect), typeof(ImageViewport),
                new FrameworkPropertyMetadata(new PxRect(0, 0, 0, 0)));

        /// <summary>
        /// 当前视口在图像像素坐标中的矩形区域（只读依赖属性）。
        /// </summary>
        public static readonly DependencyProperty ImageViewRectProperty = ImageViewRectPropertyKey.DependencyProperty;

        /// <summary>
        /// 获取当前视口在图像像素坐标中的矩形区域。
        /// </summary>
        public PxRect ImageViewRect => (PxRect)GetValue(ImageViewRectProperty);
        #endregion

        #region 视口窗口像素尺寸
        static readonly DependencyPropertyKey WindowPixelSizePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(WindowPixelSize), typeof(PxSize), typeof(ImageViewport),
                new FrameworkPropertyMetadata(new PxSize(0, 0)));

        /// <summary>
        /// 视口窗口像素尺寸（只读依赖属性）。
        /// </summary>
        public static readonly DependencyProperty WindowPixelSizeProperty = WindowPixelSizePropertyKey.DependencyProperty;

        /// <summary>
        /// 获取视口窗口像素尺寸。
        /// </summary>
        public PxSize WindowPixelSize => (PxSize)GetValue(WindowPixelSizeProperty);
        #endregion

        #region 缩放相关

        #region 当前缩放比例
        static readonly DependencyPropertyKey ScalePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Scale), typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0, null, CoerceScaleValue));

        /// <summary>
        /// 强制回调，当 Scale 属性被内部更新时，同步更新 TargetScale。
        /// </summary>
        private static object CoerceScaleValue(DependencyObject d, object baseValue)
        {
            // 当后端服务更新 Scale 时，此回调被触发。
            // 我们利用这个时机将新值同步到 TargetScale，以实现双向绑定的"反向"更新。
            var owner = (ImageViewport)d;
            var newScale = (double)baseValue;

            // 仅在值不同时更新，避免不必要的循环
            if (Math.Abs(owner.TargetScale - newScale) > 1e-9)
            {
                owner.TargetScale = newScale;
            }

            return baseValue;
        }

        /// <summary>
        /// 当前缩放比例（只读依赖属性）。
        /// </summary>
        public static readonly DependencyProperty ScaleProperty = ScalePropertyKey.DependencyProperty;

        /// <summary>
        /// 获取当前缩放比例。
        /// </summary>
        public double Scale => (double)GetValue(ScaleProperty);
        #endregion

        #region 目标缩放比例
        /// <summary>
        /// 目标缩放比例（可绑定，双向）。
        /// </summary>
        public static readonly DependencyProperty TargetScaleProperty =
            DependencyProperty.Register(nameof(TargetScale), typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((ImageViewport)d).OnTargetScaleChanged((double)e.NewValue)));

        /// <summary>
        /// 获取或设置目标缩放比例。
        /// </summary>
        public double TargetScale
        {
            get => (double)GetValue(TargetScaleProperty);
            set => SetValue(TargetScaleProperty, value);
        }
        #endregion
        #endregion

        #region 缩放锚点模式
        /// <summary>
        /// 缩放锚点模式（可绑定，双向）。
        /// </summary>
        public static readonly DependencyProperty ZoomPivotModeProperty =
            DependencyProperty.Register(nameof(ZoomPivotModeSetting), typeof(ZoomPivotMode), typeof(ImageViewport),
                new FrameworkPropertyMetadata(ZoomPivotMode.Mouse, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 获取或设置缩放锚点模式。
        /// </summary>
        public ZoomPivotMode ZoomPivotModeSetting
        {
            get => (ZoomPivotMode)GetValue(ZoomPivotModeProperty);
            set => SetValue(ZoomPivotModeProperty, value);
        }
        #endregion

        #region 自定义缩放锚点

        /// <summary>
        /// 自定义缩放锚点的窗口像素坐标（可绑定，双向）。
        /// </summary>
        public static readonly DependencyProperty ZoomPivotWindowPxProperty =
            DependencyProperty.Register(nameof(ZoomPivotWindowPx), typeof(PxPoint), typeof(ImageViewport),
                new FrameworkPropertyMetadata(new PxPoint(0, 0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 获取或设置自定义缩放锚点的窗口像素坐标。
        /// </summary>
        public PxPoint ZoomPivotWindowPx
        {
            get => (PxPoint)GetValue(ZoomPivotWindowPxProperty);
            set => SetValue(ZoomPivotWindowPxProperty, value);
        }
        #endregion

        #region 缩放范围

        /// <summary>
        /// 最小缩放比例（可绑定，双向）。
        /// </summary>
        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register(nameof(MinScale), typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(0.01, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 获取或设置最小缩放比例。
        /// </summary>
        public double MinScale
        {
            get => (double)GetValue(MinScaleProperty);
            set => SetValue(MinScaleProperty, value);
        }

        /// <summary>
        /// 最大缩放比例（可绑定，双向）。
        /// </summary>
        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register(nameof(MaxScale), typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 获取或设置最大缩放比例。
        /// </summary>
        public double MaxScale
        {
            get => (double)GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
        }
        #endregion

        #region 滚轮缩放因子
        /// <summary>
        /// 滚轮缩放因子（每 120 的 WheelDelta，缩放倍率 = ScaleFactor）。
        /// 仅用于滚轮缩放，不影响 TargetScale 绝对缩放。
        /// </summary>
        public double ScaleFactor
        {
            get => (double)GetValue(ScaleFactorProperty);
            set => SetValue(ScaleFactorProperty, value);
        }

        /// <summary>
        /// 滚轮缩放因子依赖属性。
        /// </summary>
        public static readonly DependencyProperty ScaleFactorProperty =
            DependencyProperty.Register(nameof(ScaleFactor), typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        #region X 方向 DPI 缩放
        static readonly DependencyPropertyKey DpiScaleXPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DpiScaleX), typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0));

        /// <summary>
        /// X 方向 DPI 缩放（只读依赖属性）。
        /// </summary>
        public static readonly DependencyProperty DpiScaleXProperty = DpiScaleXPropertyKey.DependencyProperty;

        /// <summary>
        /// 获取 X 方向 DPI 缩放。
        /// </summary>
        public double DpiScaleX => (double)GetValue(DpiScaleXProperty);
        #endregion

        #region Y 方向 DPI 缩放
        static readonly DependencyPropertyKey DpiScaleYPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DpiScaleY), typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0));

        /// <summary>
        /// Y 方向 DPI 缩放（只读依赖属性）。
        /// </summary>
        public static readonly DependencyProperty DpiScaleYProperty = DpiScaleYPropertyKey.DependencyProperty;

        /// <summary>
        /// 获取 Y 方向 DPI 缩放。
        /// </summary>
        public double DpiScaleY => (double)GetValue(DpiScaleYProperty);
        #endregion

        #region 控件复写

        /// <summary>
        /// 应用控件模板并绑定部件。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _host = GetTemplateChild(PartHost) as LayeredSurfaceHost
                    ?? throw new InvalidOperationException("PART_SurfaceHost not found.");

            Rebind();
        }

        /// <summary>
        /// 渲染尺寸变化时的处理。
        /// </summary>
        /// <param name="sizeInfo">尺寸变化信息。</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _service?.SetWindowSize(new PxSize(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height));
        }

        #endregion

        /// <summary>
        /// 重新绑定后端服务与宿主。
        /// </summary>
        void Rebind()
        {
            if (_host is null) return;

            _facade = Facade ?? throw new InvalidOperationException("Facade is required.");
            _backendFactory = _facade as IViewportBackendFactory;

            // 如果已有服务，先解绑
            if (_service is IViewportObservable oldObs)
            {
                oldObs.ViewportChanged -= OnViewportChangedInternal;
            }

            _service = _backendFactory?.CreateViewportService() ?? new BuiltInViewportService();
            if (_service is IViewportObservable newObs)
            {
                newObs.ViewportChanged += OnViewportChangedInternal;
            }

            DpiObserver.Attach(this, _service);

            _host.Bind(_facade, _service);
            // 初始状态由 Service 首次事件驱动
            _service.SetWindowSize(new PxSize(ActualWidth, ActualHeight));
            InvalidateVisual();
        }

        // ---------- Thin facades for routers/handlers ----------
        /// <summary>
        /// 以窗口像素为锚点进行缩放。
        /// </summary>
        /// <param name="factor">缩放因子。</param>
        /// <param name="windowPx">窗口像素坐标。</param>
        public void ZoomAtWindowPx(double factor, PxPoint windowPx)
        {
            if (factor <= 0 || _service is null) return;
            _service.ZoomAtWindowPx(factor, windowPx);
        }

        /// <summary>
        /// 以图像像素为锚点进行缩放，缩放后锚点在窗口中的位置保持不变。
        /// </summary>
        /// <param name="factor">缩放因子。</param>
        /// <param name="imagePx">图像像素坐标。</param>
        public void ZoomAtImagePx(double factor, PxPoint imagePx)
        {
            // 1. 缩放前，锚点在窗口中的位置
            var anchorWinPxBefore = ImageToWindow(imagePx);

            // 2. 以该窗口位置为锚点缩放
            ZoomAtWindowPx(factor, anchorWinPxBefore);

            // 3. 缩放后，锚点在窗口中的新位置
            var anchorWinPxAfter = ImageToWindow(imagePx);

            // 4. 做补偿平移
            var dx = anchorWinPxBefore.X - anchorWinPxAfter.X;
            var dy = anchorWinPxBefore.Y - anchorWinPxAfter.Y;
            if (System.Math.Abs(dx) > 0.01 || System.Math.Abs(dy) > 0.01)
                PanWindowPx(dx, dy);
        }

        /// <summary>
        /// 以窗口像素为单位进行平移。
        /// </summary>
        /// <param name="dx">水平像素偏移。</param>
        /// <param name="dy">垂直像素偏移。</param>
        public void PanWindowPx(double dx, double dy)
        {
            if (_service is null || (dx == 0 && dy == 0)) return;
            _service.PanWindowPx(dx, dy);
        }

        /// <summary>
        /// 将图像矩形适配到视口（直接调用后端 Service）。
        /// </summary>
        /// <param name="imageRect">图像像素矩形。</param>
        public void FitImageRect(PxRect imageRect)
        {
            if (_service is null) return;

            if (double.IsNaN(imageRect.X) || double.IsNaN(imageRect.Y)
                || double.IsNaN(imageRect.Width) || double.IsNaN(imageRect.Height)
                || imageRect.Width <= 0 || imageRect.Height <= 0)
            {
                return;
            }

            _fitRectCache = imageRect;
            _service.FitImageRect(imageRect);
        }

        // 新增无参重载：使用缓存
        public void FitImageRect()
        {
            if (_service is null || _fitRectCache is null) return;
            _service.FitImageRect(_fitRectCache.Value);
        }

        /// <summary>
        /// 窗口像素→图像像素坐标。
        /// </summary>
        public PxPoint WindowToImage(PxPoint windowPx)
        {
            if (_service is null) return new PxPoint();
            var tf = (_backendFactory?.CreateTransforms(_service.Snapshot())) ?? new BuiltInViewportTransforms(_service.Snapshot());
            return tf.WindowToImage(windowPx);
        }

        /// <summary>
        /// 图像像素→窗口像素坐标。
        /// </summary>
        public PxPoint ImageToWindow(PxPoint imagePx)
        {
            if (_service is null) return new PxPoint();
            var tf = (_backendFactory?.CreateTransforms(_service.Snapshot())) ?? new BuiltInViewportTransforms(_service.Snapshot());
            return tf.ImageToWindow(imagePx);
        }

        /// <summary>
        /// 窗口矩形→图像像素矩形。
        /// </summary>
        public PxRect WindowRectToImageRect(PxRect windowRect)
        {
            if (_service is null) return new PxRect();
            var tf = (_backendFactory?.CreateTransforms(_service.Snapshot())) ?? new BuiltInViewportTransforms(_service.Snapshot());
            return tf.WindowToImage(windowRect);
        }

        /// <summary>
        /// 图像像素矩形→窗口矩形。
        /// </summary>
        public PxRect ImageRectToWindowRect(PxRect imageRect)
        {
            if (_service is null) return new PxRect();
            var tf = (_backendFactory?.CreateTransforms(_service.Snapshot())) ?? new BuiltInViewportTransforms(_service.Snapshot());
            return tf.ImageToWindow(imageRect);
        }

        /// <summary>
        /// 目标缩放比例发生变化时的处理逻辑。
        /// </summary>
        void OnTargetScaleChanged(double newScale)
        {
            if (_service is null || double.IsNaN(newScale) || newScale <= 0) return;
            var snap = _service.Snapshot();
            var current = snap.Scale <= 0 ? 1.0 : snap.Scale;

            double desired = newScale;
            var step = ScaleFactor;
            if (!double.IsNaN(step) && step > 1.0)
            {
                var n = Math.Round(Math.Log(desired / current, step));
                desired = current * Math.Pow(step, n);
            }

            var minS = MinScale > 0 ? MinScale : 1e-6;
            var maxS = MaxScale > minS ? MaxScale : minS;
            desired = Math.Max(minS, Math.Min(maxS, desired));

            var factor = desired / current;
            if (factor <= 0 || Math.Abs(factor - 1.0) < 1e-9) return;

            double px = snap.WindowPixelSize.Width / 2.0;
            double py = snap.WindowPixelSize.Height / 2.0;
            switch (ZoomPivotModeSetting)
            {
                case ZoomPivotMode.Mouse:
                    if (_lastMoveWindowPx.HasValue) { px = _lastMoveWindowPx.Value.X; py = _lastMoveWindowPx.Value.Y; }
                    break;
                case ZoomPivotMode.Custom:
                    px = ZoomPivotWindowPx.X; py = ZoomPivotWindowPx.Y;
                    break;
                case ZoomPivotMode.Center:
                default:
                    // 修复：使用图像中心而不是窗口中心
                    var imageCenterPx = new PxPoint(
                        snap.ViewportRectInImage.X + snap.ViewportRectInImage.Width / 2.0,
                        snap.ViewportRectInImage.Y + snap.ViewportRectInImage.Height / 2.0
                    );
                    var windowCenter = ImageToWindow(imageCenterPx);
                    px = windowCenter.X;
                    py = windowCenter.Y;
                    break;
            }

            _service.ZoomAtWindowPx(factor, new PxPoint(px, py));
        }

        /// <summary>
        /// 视口状态变更事件处理。
        /// </summary>
        void OnViewportChangedInternal(object? sender, ViewportInfo e)
        {
            // 唯一的入口点，用于更新所有依赖属性和视图
            // 使用 Dispatcher.CheckAccess() 可以在未来支持多线程后端
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ApplyViewportInfo(e));
                return;
            }
            ApplyViewportInfo(e);
        }

        /// <summary>
        /// 将视口信息应用到控件的依赖属性和矩阵。
        /// </summary>
        void ApplyViewportInfo(ViewportInfo e)
        {
            // 1. 更新依赖属性
            var oldScale = Scale;
            var oldImageViewRect = ImageViewRect;
            var oldWindowPixelSize = WindowPixelSize;
            var oldDpiScaleX = DpiScaleX;
            var oldDpiScaleY = DpiScaleY;

            SetValue(ScalePropertyKey, e.Scale);
            SetValue(ImageViewRectPropertyKey, e.ViewportRectInImage);
            SetValue(WindowPixelSizePropertyKey, e.WindowPixelSize);
            SetValue(DpiScaleXPropertyKey, e.DpiScaleX);
            SetValue(DpiScaleYPropertyKey, e.DpiScaleY);

            // 2. 更新变换矩阵
            var s = e.Scale;
            var tl = e.ViewportRectInImage.TopLeft;
            _viewportMatrix.Matrix = new Matrix(s, 0, 0, s, -tl.X * s, -tl.Y * s);

            // 3. 触发公开事件（如果值有变化）
            if (Math.Abs(oldScale - e.Scale) > double.Epsilon)
                ScaleChanged?.Invoke(this, new ScaleChangedEventArgs(e.Scale));

            if (oldImageViewRect != e.ViewportRectInImage)
                PanChanged?.Invoke(this, new PanChangedEventArgs(e.ViewportRectInImage.TopLeft));

            if (oldWindowPixelSize != e.WindowPixelSize)
                WindowPixelSizeChanged?.Invoke(this, new WindowPixelSizeChangedEventArgs(e.WindowPixelSize));

            if (Math.Abs(oldDpiScaleX - e.DpiScaleX) > double.Epsilon || Math.Abs(oldDpiScaleY - e.DpiScaleY) > double.Epsilon)
                DpiScaleChanged?.Invoke(this, new DpiScaleChangedEventArgs(e.DpiScaleX, e.DpiScaleY));

            ViewportChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 获取当前有效的输入路由器。
        /// </summary>
        /// <returns>输入路由器实例。</returns>
        IInputRouter GetEffectiveRouter()
        {
            return _facade?.InputRouter ?? (_defaultRouter ??= NoopRouter);
        }

        #region 静态只读空路由器
        // 在 ImageViewport 类里增加一个静态只读空路由器
        static readonly IInputRouter NoopRouter = new NoopInputRouter();

        // 极简空实现：不拦截任何输入（控件默认无交互）
        private sealed class NoopInputRouter : IInputRouter
        {
            public bool OnWheel(object s, PointerEvent p) => false;
            public bool OnMove(object s, PointerEvent p) => false;
            public bool OnLeftDown(object s, PointerEvent p) => false;
            public bool OnLeftUp(object s, PointerEvent p) => false;
            public bool OnRightDown(object s, PointerEvent p) => false;
            public bool OnRightUp(object s, PointerEvent p) => false;
            public bool OnMouseDown(object s, PointerEvent p) => false;
            public bool OnMouseUp(object s, PointerEvent p) => false;
            public bool OnMiddleDown(object sender, PointerEvent p) => false;
            public bool OnMiddleUp(object sender, PointerEvent p) => false;
        }
        #endregion

        // Events

        /// <summary>
        /// 视口信息变更事件。
        /// </summary>
        public event EventHandler<ViewportInfo>? ViewportChanged;
        /// <summary>
        /// 窗口像素尺寸变更事件。
        /// </summary>
        public event EventHandler<WindowPixelSizeChangedEventArgs>? WindowPixelSizeChanged;
        /// <summary>
        /// 缩放比例变更事件。
        /// </summary>
        public event EventHandler<ScaleChangedEventArgs>? ScaleChanged;
        /// <summary>
        /// 平移变更事件。
        /// </summary>
        public event EventHandler<PanChangedEventArgs>? PanChanged;
        /// <summary>
        /// DPI 缩放变更事件。
        /// </summary>
        public event EventHandler<DpiScaleChangedEventArgs>? DpiScaleChanged;
        /// <summary>
        /// 请求弹出上下文菜单事件。
        /// </summary>
        public event EventHandler<PointerEvent>? ContextMenuRequested;

        #region Input + PointerEvent

        /// <summary>
        /// 构建指针事件信息。
        /// </summary>
        /// <param name="e">鼠标事件参数。</param>
        /// <param name="winPt">窗口坐标。</param>
        /// <param name="wheelDelta">滚轮增量。</param>
        /// <param name="leftDownSnapshot">左键是否按下。</param>
        /// <param name="rightDownSnapshot">右键是否按下。</param>
        /// <param name="middleDownSnapshot">中键是否按下。</param>
        /// <returns>指针事件。</returns>
        PointerEvent BuildPointerEvent(MouseEventArgs e, Point winPt, double wheelDelta = 0,
            bool leftDownSnapshot = false, bool rightDownSnapshot = false, bool middleDownSnapshot = false)
        {
            if (_service is null) return new PointerEvent();
            var view = _service.Snapshot();
            PxPoint pxWin = new(winPt.X, winPt.Y);
            PxPoint pxImg = new(view.ViewportRectInImage.X + pxWin.X / view.Scale,
                                view.ViewportRectInImage.Y + pxWin.Y / view.Scale);

            var mods = ModifierKeys.None;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) mods |= ModifierKeys.Control;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) mods |= ModifierKeys.Shift;
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) mods |= ModifierKeys.Alt;

            return new PointerEvent
            {
                Timestamp = DateTime.UtcNow,
                WindowPx = pxWin,
                ImagePx = pxImg,
                IsLeftDown = leftDownSnapshot,
                IsRightDown = rightDownSnapshot,
                IsMiddleDown = middleDownSnapshot,
                WheelDelta = wheelDelta,
                ClickCount = 0,
                Modifiers = mods,
                CurrentLeftPressed = e.LeftButton == MouseButtonState.Pressed,
                CurrentRightPressed = e.RightButton == MouseButtonState.Pressed,
                CurrentMiddlePressed = e.MiddleButton == MouseButtonState.Pressed
            };
        }

        /// <summary>
        /// 鼠标滚轮事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_host is null) return;
            var router = GetEffectiveRouter();
            var p = e.GetPosition(_host);
            _lastMoveWindowPx = p;
            var pe = BuildPointerEvent(e, p, wheelDelta: e.Delta, leftDownSnapshot: _isLeftDown, rightDownSnapshot: _isRightDown, middleDownSnapshot: _isMiddleDown);
            if (router.OnWheel(this, pe)) { _host.Invalidate(); e.Handled = true; }
        }

        /// <summary>
        /// 鼠标移动事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_host is null)
            {
                return;
            }

            var p = e.GetPosition(_host);
            var pe = BuildPointerEvent(e, p, leftDownSnapshot: _isLeftDown, rightDownSnapshot: _isRightDown, middleDownSnapshot: _isMiddleDown);
            _lastMoveWindowPx = p;

            var router = GetEffectiveRouter();
            bool handled = router.OnMove(this, pe);
            if (handled)
            {
                _host.Invalidate();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 鼠标左键按下事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (_host is null) return;
            var router = GetEffectiveRouter();
            Focus();
            _isLeftDown = true;
            _clickCountLeft = e.ClickCount;
            var p = e.GetPosition(_host);
            _lastMoveWindowPx = p;
            var pe = BuildPointerEvent(e, p, leftDownSnapshot: true, rightDownSnapshot: _isRightDown, middleDownSnapshot: _isMiddleDown);
            bool handled = router.OnLeftDown(this, pe) || router.OnMouseDown(this, pe);
            if (handled) { _host.Invalidate(); e.Handled = true; }
        }

        /// <summary>
        /// 鼠标左键释放事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (_host is null) return;
            var router = GetEffectiveRouter();
            _isLeftDown = false;
            var p = e.GetPosition(_host);
            _lastMoveWindowPx = p;
            var pe = BuildPointerEvent(e, p, leftDownSnapshot: false, rightDownSnapshot: _isRightDown, middleDownSnapshot: _isMiddleDown);
            bool handled = router.OnLeftUp(this, pe) || router.OnMouseUp(this, pe);
            if (handled) { _host.Invalidate(); e.Handled = true; }
        }

        /// <summary>
        /// 鼠标右键按下事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            if (_host is null) return;
            var router = GetEffectiveRouter();
            _isRightDown = true;
            _clickCountRight = e.ClickCount;
            var p = e.GetPosition(_host);
            _lastMoveWindowPx = p;
            var pe = BuildPointerEvent(e, p, leftDownSnapshot: _isLeftDown, rightDownSnapshot: true, middleDownSnapshot: _isMiddleDown);
            bool handled = router.OnRightDown(this, pe) || router.OnMouseDown(this, pe);
            if (handled) { _host.Invalidate(); e.Handled = true; }
        }

        /// <summary>
        /// 鼠标右键释放事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            var p = _host is null ? new Point(0, 0) : e.GetPosition(_host);
            _isRightDown = false;
            var pe = BuildPointerEvent(e, p, leftDownSnapshot: _isLeftDown, rightDownSnapshot: false, middleDownSnapshot: _isMiddleDown);
            bool handled = _facade?.InputRouter?.OnRightUp(this, pe) == true || _facade?.InputRouter?.OnMouseUp(this, pe) == true;
            if (!handled)
            {
                ContextMenuRequested?.Invoke(this, pe);
                var menu = _facade?.ContextMenu?.BuildContextMenu(pe);
                if (menu != null) { menu.PlacementTarget = this; menu.IsOpen = true; handled = true; }
            }
            if (handled) { _host?.Invalidate(); e.Handled = true; }
        }

        /// <summary>
        /// 鼠标按下事件处理（支持三键）。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (_host is null) return;
            var router = GetEffectiveRouter();
            var p = e.GetPosition(_host);

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    _isLeftDown = true;
                    _clickCountLeft = e.ClickCount;
                    _lastMoveWindowPx = p;
                    break;
                case MouseButton.Right:
                    _isRightDown = true;
                    _clickCountRight = e.ClickCount;
                    _lastMoveWindowPx = p;
                    break;
                case MouseButton.Middle:
                    _isMiddleDown = true;
                    _lastMoveWindowPx = p;
                    break;
            }

            var pe = BuildPointerEvent(
                e, p,
                leftDownSnapshot: _isLeftDown,
                rightDownSnapshot: _isRightDown,
                middleDownSnapshot: _isMiddleDown
            );
            bool handled = router.OnMouseDown(this, pe);
            if (handled) { _host.Invalidate(); e.Handled = true; }
        }

        /// <summary>
        /// 鼠标释放事件处理（支持三键）。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (_host is null) return;
            var router = GetEffectiveRouter();
            var p = e.GetPosition(_host);

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    _isLeftDown = false;
                    break;
                case MouseButton.Right:
                    _isRightDown = false;
                    break;
                case MouseButton.Middle:
                    _isMiddleDown = false;
                    break;
            }

            _lastMoveWindowPx = p;
            var pe = BuildPointerEvent(
                e, p,
                leftDownSnapshot: _isLeftDown,
                rightDownSnapshot: _isRightDown,
                middleDownSnapshot: _isMiddleDown
            );
            bool handled = router.OnMouseUp(this, pe);
            if (handled) { _host.Invalidate(); e.Handled = true; }
        }

        #endregion
    }
}
