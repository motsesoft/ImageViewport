using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Runtime.Services
{
    /// <summary>
    /// 内置视口服务实现。
    /// 提供基础的视口操作功能，包括缩放、平移、适应区域等。
    /// 实现了 <see cref="IViewportObservable"/> 接口以支持状态变化通知。
    /// </summary>
    internal sealed class BuiltInViewportService : IViewportService, IViewportObservable
    {
        private PxSize _windowSize = new(0, 0);
        private PxRect _imageViewRect = new(0, 0, 100, 100);
        private double _scale = 1.0;
        private double _dpiScaleX = 1.0, _dpiScaleY = 1.0;

        /// <summary>
        /// 视口状态发生变化时触发的事件。
        /// </summary>
        public event EventHandler<ViewportInfo>? ViewportChanged;

        /// <summary>
        /// 获取当前视口状态的快照。
        /// </summary>
        /// <returns>包含当前视口状态的 <see cref="ViewportInfo"/> 实例。</returns>
        public ViewportInfo Snapshot() => new()
        {
            WindowPixelSize = _windowSize,
            ViewportRectInImage = _imageViewRect,
            Scale = _scale,
            DpiScaleX = _dpiScaleX,
            DpiScaleY = _dpiScaleY
        };

        /// <summary>
        /// 以窗口像素为基准，在指定点进行缩放操作。
        /// </summary>
        /// <param name="scaleFactor">缩放因子。</param>
        /// <param name="windowPx">窗口像素坐标，作为缩放中心。</param>
        public void ZoomAtWindowPx(double scaleFactor, PxPoint windowPx)
        {
            if (scaleFactor <= 0) return;

            var oldScale = _scale;
            _scale *= scaleFactor;

            // 计算缩放中心在图像坐标系中的位置
            var centerX = _imageViewRect.X + windowPx.X / oldScale;
            var centerY = _imageViewRect.Y + windowPx.Y / oldScale;

            // 重新计算视口在图像坐标系中的位置，使缩放中心保持不变
            _imageViewRect = new PxRect(
                centerX - _windowSize.Width / _scale / 2.0,
                centerY - _windowSize.Height / _scale / 2.0,
                _windowSize.Width / _scale,
                _windowSize.Height / _scale);

            RaiseViewportChanged();
        }

        /// <summary>
        /// 以窗口像素为单位进行平移操作。
        /// </summary>
        /// <param name="dx">水平方向平移的像素数。</param>
        /// <param name="dy">垂直方向平移的像素数。</param>
        public void PanWindowPx(double dx, double dy)
        {
            if (dx == 0 && dy == 0) return;

            // 将窗口坐标的平移转换为图像坐标的平移
            _imageViewRect = new PxRect(
                _imageViewRect.X - dx / _scale,
                _imageViewRect.Y - dy / _scale,
                _imageViewRect.Width,
                _imageViewRect.Height);

            RaiseViewportChanged();
        }

        /// <summary>
        /// 使视口适应指定的图像像素区域。
        /// </summary>
        /// <param name="imagePxRect">需要适应的图像像素区域。</param>
        public void FitImageRect(PxRect imagePxRect)
        {
            // 确保窗口和图像尺寸有效，防止除零错误
            if (_windowSize.Width <= 0 || _windowSize.Height <= 0 ||
                imagePxRect.Width <= 0 || imagePxRect.Height <= 0)
            {
                return;
            }

            // 1. 计算水平和垂直方向的缩放比例
            var scaleX = _windowSize.Width / imagePxRect.Width;
            var scaleY = _windowSize.Height / imagePxRect.Height;

            // 2. 取较小的比例以确保整个图像区域都能被容纳
            _scale = Math.Min(scaleX, scaleY);

            // 3. 基于新比例计算视口在图像坐标系下的新尺寸
            var newViewWidth = _windowSize.Width / _scale;
            var newViewHeight = _windowSize.Height / _scale;

            // 4. 计算左上角坐标，使图像区域在视口中居中显示
            var newX = imagePxRect.X + (imagePxRect.Width - newViewWidth) / 2.0;
            var newY = imagePxRect.Y + (imagePxRect.Height - newViewHeight) / 2.0;

            _imageViewRect = new PxRect(newX, newY, newViewWidth, newViewHeight);
            RaiseViewportChanged();
        }

        /// <summary>
        /// 设置视口的窗口尺寸（以像素为单位）。
        /// </summary>
        /// <param name="size">窗口像素尺寸。</param>
        public void SetWindowSize(PxSize size)
        {
            _windowSize = size;

            // 更新视口在图像坐标系中的尺寸，保持当前的左上角位置
            _imageViewRect = new PxRect(
                _imageViewRect.X,
                _imageViewRect.Y,
                size.Width / _scale,
                size.Height / _scale);

            RaiseViewportChanged();
        }

        /// <summary>
        /// 设置视口的 DPI 缩放比例。
        /// </summary>
        /// <param name="x">X 方向的 DPI 缩放比例。</param>
        /// <param name="y">Y 方向的 DPI 缩放比例。</param>
        public void SetDpi(double x, double y)
        {
            _dpiScaleX = x;
            _dpiScaleY = y;
            RaiseViewportChanged();
        }

        /// <summary>
        /// 触发视口变化事件。
        /// </summary>
        private void RaiseViewportChanged() => ViewportChanged?.Invoke(this, Snapshot());
    }
}