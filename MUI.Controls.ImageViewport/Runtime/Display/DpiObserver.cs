using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Runtime.Display
{
    /// <summary>
    /// DPI 观察器，用于监控和响应 DPI 变化事件。
    /// 支持高 DPI 显示环境下的自适应缩放。
    /// </summary>
    internal static class DpiObserver
    {
        /// <summary>
        /// 将 DPI 观察器附加到指定的视觉元素和视口服务。
        /// </summary>
        /// <param name="visual">要监控的视觉元素。</param>
        /// <param name="service">视口服务实例。</param>
        public static void Attach(Visual visual, IViewportService service)
        {
            if (visual == null) return;

            try
            {
                // 获取当前 DPI 设置
                var dpi = VisualTreeHelper.GetDpi(visual);
                service.SetDpi(dpi.DpiScaleX, dpi.DpiScaleY);

                // 尝试监听 DPI 变化事件
                var src = PresentationSource.FromVisual(visual);
                if (src is HwndSource hwnd && hwnd.CompositionTarget is HwndTarget target)
                {
                    // DpiChanged 事件在较新的 WPF 版本中可用；如果不存在则静默回退
                    var evt = target.GetType().GetEvent("DpiChanged");
                    if (evt != null)
                    {
                        EventHandler handler = (s, e) =>
                        {
                            var d = VisualTreeHelper.GetDpi(visual);
                            service.SetDpi(d.DpiScaleX, d.DpiScaleY);
                        };
                        evt.AddEventHandler(target, handler);
                    }
                }
            } catch
            {
                // 最大努力原则；如果环境不支持 DPI 查询则忽略
            }
        }
    }
}