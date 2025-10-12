using System;
using MUI.Controls.ImageViewport;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;

namespace MUI.Controls.ImageViewport.Handlers
{
    /// <summary>
    /// 支持左键/中键/右键拖拽平移、滚轮缩放；通过 IPanZoomConfig 获取配置。
    /// </summary>
    public sealed class PanZoomHandler : IInputRouter
    {
        private PxPoint? _lastWin;
        private readonly IPanZoomConfig _cfg;

        public PanZoomHandler(IPanZoomConfig cfg) => _cfg = cfg;

        private bool ShouldPan(PointerEvent p) =>
            (_cfg.PanButton == PanButton.Left   && p.IsLeftDown) ||
            (_cfg.PanButton == PanButton.Middle && p.IsMiddleDown) ||
            (_cfg.PanButton == PanButton.Right  && p.IsRightDown);

        public bool OnLeftDown(object sender, PointerEvent p) { if (_cfg.PanButton == PanButton.Left)   _lastWin = p.WindowPx; return false; }
        public bool OnRightDown(object sender, PointerEvent p){ if (_cfg.PanButton == PanButton.Right)  _lastWin = p.WindowPx; return false; }
        public bool OnLeftUp(object sender, PointerEvent p)   { if (_cfg.PanButton == PanButton.Left)   _lastWin = null; return false; }
        public bool OnRightUp(object sender, PointerEvent p)  { if (_cfg.PanButton == PanButton.Right)  _lastWin = null; return false; }
        public bool OnMouseDown(object sender, PointerEvent p) => false;
        public bool OnMouseUp(object sender, PointerEvent p) => false;

        public bool OnMove(object sender, PointerEvent p)
        {
            if (!ShouldPan(p) || !p.IsDragging) return false;
            if (_lastWin is null) _lastWin = p.WindowPx;

            if (sender is ImageViewport vp)
            {
                var dx = p.WindowPx.X - _lastWin.Value.X;
                var dy = p.WindowPx.Y - _lastWin.Value.Y;
                vp.PanWindowPx(dx, dy);
                _lastWin = p.WindowPx;
                return true;
            }
            return false;
        }

        public bool OnWheel(object sender, PointerEvent p)
        {
            if (sender is not MUI.Controls.ImageViewport.ImageViewport vp) return false;
            if (_cfg.RequireCtrlForWheelZoom && (p.Modifiers & System.Windows.Input.ModifierKeys.Control) == 0)
                return false;

            var factorPerTick = _cfg.ScaleFactorOverride.HasValue ? System.Math.Max(1.0000001, _cfg.ScaleFactorOverride.Value) : (vp.ScaleFactor > 1.0 ? vp.ScaleFactor : 1.1);
        double ticks = p.WheelDelta / 120.0;
            if (System.Math.Abs(ticks) < 1e-6) return false;

            var current = vp.Scale <= 0 ? 1.0 : vp.Scale;
            var desired = current * System.Math.Pow(factorPerTick, ticks);

            var minS = vp.MinScale > 0 ? vp.MinScale : 1e-6;
            var maxS = vp.MaxScale > minS ? vp.MaxScale : minS;
            desired = System.Math.Max(minS, System.Math.Min(maxS, desired));

            var factor = desired / current;
            if (System.Math.Abs(factor - 1.0) < 1e-9) return false;

            vp.ZoomAtWindowPx(factor, p.WindowPx);
            return true;
        }
    }
}