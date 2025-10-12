using System.Windows;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Facade;
using MUI.Controls.ImageViewport.Contracts.Surfaces;
using MUI.Controls.ImageViewport.Runtime.Transforms;

namespace MUI.Controls.ImageViewport
{
    /// <summary>
    /// 分层表面宿主，负责管理和渲染多个图层表面。
    /// 支持跟随视口变换和独立变换两种渲染模式。
    /// </summary>
    public sealed class LayeredSurfaceHost : FrameworkElement
    {
        private IViewportFacade? _facade;
        private IViewportService? _service;

        /// <summary>
        /// 绑定视口外观和服务。
        /// </summary>
        /// <param name="facade">视口外观接口。</param>
        /// <param name="service">视口服务接口。</param>
        public void Bind(IViewportFacade facade, IViewportService service)
        {
            _facade = facade;
            _service = service;
            InvalidateVisual();
        }

        /// <summary>
        /// 渲染所有图层表面。
        /// </summary>
        /// <param name="dc">绘图上下文。</param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_facade is null || _service is null)
                return;

            var view = _service.Snapshot();
            var mapper = new BuiltInViewportTransforms(view);
            var winRect = new Rect(RenderSize);

            // 构建视口矩阵和逆矩阵
            var s = view.Scale;
            var tl = view.ViewportRectInImage.TopLeft;
            var viewportMatrix = new Matrix(s, 0, 0, s, -tl.X * s, -tl.Y * s);
            var viewportMatrixInverse = viewportMatrix;
            viewportMatrixInverse.Invert();

            // 创建渲染上下文
            var ctx = new SurfaceRenderContext(
                winRect,
                view,
                mapper,
                viewportMatrix,
                viewportMatrixInverse,
                view.DpiScaleX,
                view.DpiScaleY
            );

            var matrixTransform = new MatrixTransform(viewportMatrix);
            bool transformPushed = false;

            // 按顺序渲染所有表面
            foreach (var layer in _facade.Surfaces)
            {
                if (layer.TransformMode == SurfaceMode.Follow)
                {
                    // Follow 模式：应用视口变换矩阵
                    if (!transformPushed)
                    {
                        dc.PushTransform(matrixTransform);
                        transformPushed = true;
                    }
                    layer.Render(dc, in ctx);
                } else // Independent 模式
                {
                    // Independent 模式：不应用变换，表面自己处理坐标
                    if (transformPushed)
                    {
                        dc.Pop();
                        transformPushed = false;
                    }
                    layer.Render(dc, in ctx);
                }
            }

            // 确保变换栈平衡
            if (transformPushed)
                dc.Pop();
        }

        /// <summary>
        /// 请求重新绘制视觉元素。
        /// </summary>
        public void Invalidate() => InvalidateVisual();
    }
}