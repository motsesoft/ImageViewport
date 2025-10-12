
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;
using Samples.ImageViewport.DemoApp.ViewModels;

namespace Samples.ImageViewport.DemoApp.Adapters
{
    /// <summary>Demo-only router: tracks mouse position in window & scene and writes to VM.</summary>
    public sealed class MouseTrackRouter : IInputRouter
    {
        private readonly ViewportViewModel _vm;
        public MouseTrackRouter(ViewportViewModel vm){ _vm = vm; }
        public bool OnMove(object sender, PointerEvent p)
        {
            _vm.MouseWindow = p.WindowPx;
            _vm.MouseScene = p.ImagePx; // ImagePx == Scene coordinate by design
            return false;
        }
        public bool OnLeftDown(object sender, PointerEvent p) => false;
        public bool OnLeftUp(object sender, PointerEvent p) => false;
        public bool OnRightDown(object sender, PointerEvent p) => false;
        public bool OnRightUp(object sender, PointerEvent p) => false;
        public bool OnWheel(object sender, PointerEvent p) => false;
        public bool OnMouseDown(object sender, PointerEvent p) => false;
        public bool OnMouseUp(object sender, PointerEvent p) => false;
    }
}
