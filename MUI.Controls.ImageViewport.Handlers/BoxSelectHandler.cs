using System;
using MUI.Controls.ImageViewport;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;

namespace MUI.Controls.ImageViewport.Handlers
{
    /// <summary>
    /// 框选：按下指定修饰键(默认 Shift)+左键拖动。通过 ISelectionSink 通知外界变化与提交。
    /// </summary>
    public sealed class BoxSelectHandler : IInputRouter
    {
        private readonly ISelectionSink _sink;
        private readonly BoxSelectOptions _opt;
        private PxPoint? _anchorWin;

        public BoxSelectHandler(ISelectionSink sink, BoxSelectOptions? options = null)
        {
            _sink = sink;
            _opt = options ?? new BoxSelectOptions();
        }

        public bool OnLeftDown(object sender, PointerEvent p)
        {
            if ((p.Modifiers & _opt.ActivationModifier) != 0)
            {
                _anchorWin = p.WindowPx;
                _sink.OnSelectionChanging(new PxRect(p.WindowPx.X, p.WindowPx.Y, 0, 0));
                return true;
            }
            return false;
        }

        public bool OnMove(object sender, PointerEvent p)
        {
            if (_anchorWin is null) return false;
            var (x0, y0) = (_anchorWin.Value.X, _anchorWin.Value.Y);
            var x = Math.Min(x0, p.WindowPx.X);
            var y = Math.Min(y0, p.WindowPx.Y);
            var w = Math.Abs(p.WindowPx.X - x0);
            var h = Math.Abs(p.WindowPx.Y - y0);
            _sink.OnSelectionChanging(new PxRect(x, y, w, h));
            return true;
        }

        public bool OnLeftUp(object sender, PointerEvent p)
        {
            if (_anchorWin is null) return false;
            var anchor = _anchorWin.Value;
            _anchorWin = null;

            if (sender is ImageViewport vp)
            {
                var x = Math.Min(anchor.X, p.WindowPx.X);
                var y = Math.Min(anchor.Y, p.WindowPx.Y);
                var w = Math.Abs(p.WindowPx.X - anchor.X);
                var h = Math.Abs(p.WindowPx.Y - anchor.Y);
                var winRect = new PxRect(x, y, w, h);
                var imgRect = vp.WindowRectToImageRect(winRect);
                if (_opt.CommitOnMouseUp) _sink.OnSelectionCommitted(winRect, imgRect);
                else _sink.OnSelectionChanging(winRect);
                return true;
            }
            return false;
        }

        public bool OnRightDown(object sender, PointerEvent p) => false;
        public bool OnRightUp(object sender, PointerEvent p) => false;
        public bool OnMouseDown(object sender, PointerEvent p) => false;
        public bool OnMouseUp(object sender, PointerEvent p) => false;
        public bool OnWheel(object sender, PointerEvent p) => false;
    }
}